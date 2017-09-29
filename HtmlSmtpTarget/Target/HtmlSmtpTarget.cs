using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading;
using NLog.Common;
using NLog.Config;
using NLog.HtmlSmtpTarget.Properties;
using NLog.HtmlSmtpTarget.Target.Utils;
using NLog.Layouts;
using NLog.Targets;
using System.ComponentModel;
using System.IO;
using HandlebarsDotNet;
using NLog.HtmlSmtpTarget.Target.HtmlSmtp;

namespace NLog.HtmlSmtpTarget.Target
{
    using System.Linq;

    [Target("HtmlSmtp")]
    public class HtmlSmtpTarget : Targets.Target
    {
        public const string IsTriggerLoggingEvent = "IsTrigger";

        private readonly Thread _worker;
        private BlockingCollection<LogEventInfo> _queue;
        private readonly CancellationTokenSource _cancelTokenSource;
        private long _lostEvents;
        private LogLevel _triggerLevel;
        private readonly Func<object, string> _mailTemplate;

        public HtmlSmtpTarget()
        {
            PreTriggerMessages = 16;
            EventBacklog = 8192;
            MaximumEventsPerMessage = 1024;
            HolddownPeriod = new TimeSpan(0, 15, 0);
            _triggerLevel = LogLevel.Warn;
            Subject = new SimpleLayout("[${machinename}] ${processname} ${event-properties:item=TriggerEvents} of ${event-properties:item=TotalEvents} "+
                "[${event-properties:item=GroupAlertEvents},${event-properties:item=GroupWarnEvents},${event-properties:item=GroupInfoEvents} ,${event-properties:item=GroupDevEvents}]"+
                " (${event-properties:item=LostEvents} lost)"+
                "${var:name=htmlsmtp.subject.suffix}");


            From = string.Format("NLog <htmlsmtp@{0}>", Dns.GetHostName());
            Transport = new SimpleLayout(SmtpClientFactory.MakeDefaultTransport());

            Message = new SimpleLayout("${message}");
            Timestamp = new SimpleLayout("${longdate}");
            Context = new SimpleLayout("${mdlc:item=id}");
            Exception = new SimpleLayout("${exception:format=Message,ToString}");
            // Level = new SimpleLayout(@"<img style=""image-{$Level}"" alt=""${Level}"" title=""${Level}"" />");
            Level = new SimpleLayout(@"${level}");

            _mailTemplate = MakeTemplate();
            Handlebars.RegisterHelper("layout", LayoutHelper);
            _cancelTokenSource = new CancellationTokenSource();
            _worker = new Thread(Worker)
            {
                Name = "HTML SMTP Logger",
                IsBackground = true,
            };
        }


        /// <summary>
        ///     The SMTP smart host used for mail forwarding.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This parameter takes an URL of the form:
        ///         <code>
        ///       smtp://username:password@hostname:port
        ///     </code>
        ///     </para>
        /// </remarks>
        /// <example>
        ///     <code>
        ///     smtp://smtp.isp.com
        ///   </code>
        /// </example>
        public Layout Transport { get; set; }

        [RequiredParameter]
        public Layout To { get; set; }

        public Layout From { get; set; }

        public Layout ReplyTo { get; set; }

        [RequiredParameter]
        public Layout Subject { get; set; }

        public MailPriority Priority { get; set; }

        /// <summary>
        /// The layout used to render a message in the html mail body
        /// </summary>
        public Layout Message { get; set; }
        /// <summary>
        /// The layout used to render a timestamp in the html mail body
        /// </summary>
        public Layout Timestamp { get; set; }
        /// <summary>
        /// The layout used to render context information (MDC, NDC, NDLC) in the html mail body
        /// </summary>
        public Layout Context { get; set; }
        /// <summary>
        /// The layout used to render an exception in the html mail body
        /// </summary>
        public Layout Exception { get; set; }
        /// <summary>
        /// The layout used to render a level in the html mail body
        /// </summary>
        public Layout Level { get; set; }

        /// <summary>
        ///     The period of time before the next email message is able to be delivered
        ///     (unless the <see cref="MaximumEventsPerMessage" /> criteria is exceeded).
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         By default the standard parsing from a XML configuration will set
        ///         this in the units of <b>days</b>. e.g. to set 15 minutes, use a string of
        ///         the form '00:15'.
        ///         <code>
        ///       d | [d.]hh:mm[:ss[.ff]]
        ///     </code>
        ///     </para>
        /// </remarks>
        /// <seealso cref="TimeSpan.Parse" />
        [DefaultValue("0:15:0")]
        public TimeSpan HolddownPeriod { get; set; }

        /// <summary>
        ///     The maximum events that should be backlogged in the queue between the event
        ///     source and the SMTP target worker thread.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This value shouldn't need to be changed unless the logger is under significant
        ///         logging stress.
        ///     </para>
        ///     <para>
        ///         If the rate at which the SMTP logger can consume events is
        ///         exceeded then this buffer will overflow and the <see cref="LostEventsParameterName" />
        ///         paramter to the style sheet will be set with the count of events discarded.
        ///     </para>
        /// </remarks>
        [DefaultValue("8192")]
        public int EventBacklog { get; set; }

        /// <summary>
        ///     The maximum number of event messages to include in a single message
        ///     before it is sent (prior to the holddown expiring). When the logger
        ///     is under stress this allows the logger to send messages at rate that
        ///     exceeds the <see cref="HolddownPeriod" />. It also helps to ensure
        ///     that individual messages don't get too large.
        /// </summary>
        [DefaultValue("1024")]
        public int MaximumEventsPerMessage { get; set; }

        /// <summary>
        ///     The number of logging events to show prior to the triggering logging event
        ///     so that the context in which the triggering event can be determined.
        /// </summary>
        [DefaultValue("16")]
        public int PreTriggerMessages { get; set; }

        /// <summary>
        ///     The level which causes an email to be sent.
        /// </summary>
        /// <seealso cref="Threshold" />
        [DefaultValue("Warn")]
        public string TriggerLevel
        {
            get { return _triggerLevel.ToString(); }
            set { _triggerLevel = LogLevel.FromString(value); }
        }


        protected override void InitializeTarget()
        {
            InternalLogger.Debug("HTML SMTP target initialise");
            base.InitializeTarget();


            _queue = EventBacklog > 0
                ? new BlockingCollection<LogEventInfo>(EventBacklog)
                : new BlockingCollection<LogEventInfo>();

            _worker.Start();
        }

        protected override void CloseTarget()
        {
            InternalLogger.Debug("Closing HTML SMTP target");
            _cancelTokenSource.Cancel();
            if (_worker.IsAlive) // Check that the thread was started successfully
            {
                if (!_worker.Join(60 * 1000 /* ms */))
                {
                    InternalLogger.Warn("SMTP worker failed to shutdown in a timely manner");
                }
            }
            base.CloseTarget();
        }


        /// <seealso cref="WorkerState" />
        protected override void Write(LogEventInfo logEvent)
        {
            if (_cancelTokenSource.IsCancellationRequested)
            {
                return;
            }

            MergeEventProperties(logEvent);
            PrecalculateVolatileLayouts(logEvent);

            // Try to add it to the worker thread queue. If the arrival rate of 
            // logging events exceeds the delivery rate, then discard the events,
            // but keep a count of the events lost (so that we can report lost
            // events as a summary).
            if (!_queue.TryAdd(logEvent))
            {
                Interlocked.Increment(ref _lostEvents);
            }
        }


        /// <summary>
        /// A HandleBars helper to call the NLog layout renderer
        /// </summary>
        /// <remarks>
        ///   The handlebar helper called 'layout' provides one parameter which is the name of
        /// the specific layout on this target. The current log message (LogEventInfo) is
        /// then formatted based on the configured NLog Layout.
        /// </remarks>
        /// <seealso cref="https://github.com/rexm/Handlebars.Net"/>
        internal void LayoutHelper(TextWriter writer, dynamic context, object[] parameters)
        {
            if (parameters.Length >= 0)
            {
                if ("message".Equals(parameters[0] as string, StringComparison.InvariantCultureIgnoreCase))
                {
                    RenderLayout(writer, context, Message);
                }
                else if ("timestamp".Equals(parameters[0] as string, StringComparison.InvariantCultureIgnoreCase))
                {
                    RenderLayout(writer, context, Timestamp);
                }
                else if ("context".Equals(parameters[0] as string, StringComparison.InvariantCultureIgnoreCase))
                {
                    RenderLayout(writer, context, Context);
                }
                else if ("exception".Equals(parameters[0] as string, StringComparison.InvariantCultureIgnoreCase))
                {
                    RenderLayout(writer, context, Exception);
                }
                else if ("level".Equals(parameters[0] as string, StringComparison.InvariantCultureIgnoreCase))
                {
                    RenderSafeLayout(writer, context, Level);
                }
                else
                {
                    RenderLayout(writer, context, Message);
                }
            }
            else
            {
                RenderLayout(writer, context, Message);
            }
        }

        private static void RenderLayout(TextWriter writer, dynamic context, Layout layout)
        {
            LogEventInfo anEvent = context as LogEventInfo;
            if (anEvent != null && layout != null)
            {
                writer.Write(layout.Render(anEvent));
            }
            else
            {
                InternalLogger.Trace("Failed to render template layout");
            }
        }

        private static void RenderSafeLayout(TextWriter writer, dynamic context, Layout layout)
        {
            LogEventInfo anEvent = context as LogEventInfo;
            if (anEvent != null && layout != null)
            {
                writer.WriteSafeString(layout.Render(anEvent));
            }
            else
            {
                InternalLogger.Trace("Failed to render template layout");
            }
        }


        private class WorkerState
        {
            public WorkerState()
            {
                Buffer = new Queue<LogEventInfo>();
                NonTriggeringEvents = new Queue<LogEventInfo>();
                LastSend = DateTime.MinValue;
            }

            /// <summary>
            ///     The list of events to email
            /// </summary>
            public Queue<LogEventInfo> Buffer { get; private set; }

            // The non-triggering noisy events
            /// <summary>
            ///     A circular queue implemented using a Queue, where items over
            ///     a queue count are manually removed.
            /// </summary>
            /// <remarks>
            ///     Consider migrating to using a
            ///     <see cref="http://en.wikipedia.org/wiki/Circular_buffer">circular buffer</see> like
            ///     <see cref="http://code.google.com/p/ngenerics/wiki/CircularQueue">CircularQueue</see>
            ///     from the nGenerics library.
            /// </remarks>
            /// <seealso cref="http://code.google.com/p/ngenerics/wiki/CircularQueue" />
            public Queue<LogEventInfo> NonTriggeringEvents { get; private set; }

            // When the last html email message was sent
            public DateTime LastSend { get; set; }
        }

        private void Worker()
        {
            var state = new WorkerState();
            try
            {
                InternalLogger.Debug("Html smtp worker thread running");

                while (!_cancelTokenSource.IsCancellationRequested)
                {
                    LogEventInfo loggingEvent;
                    if (_queue.TryTake(
                        out loggingEvent,
                        ToMillseconds(GetWaitTimeOut(state)),
                        _cancelTokenSource.Token))
                    {
                        AddLoggingEvent(state, loggingEvent);
                    }

                    var now = DateTime.UtcNow;
                    if ((state.Buffer.Count > 0 && (state.LastSend + HolddownPeriod) < now) ||
                        state.Buffer.Count > MaximumEventsPerMessage)
                    {
                        SendEvents(state);
                    } // else we won't sent the events
                }
            }
            catch (OperationCanceledException)
            {
                InternalLogger.Debug("Cancellation request for html smtp worker thread");
                return;
            }
            catch (Exception e)
            {
                InternalLogger.Error(e, "Unexpected exception in html smtp worker: {0}", e.Message);
            }
            finally
            {
                // Flush any outstanding events
                LogEventInfo loggingEvent;
                while (_queue.TryTake(out loggingEvent))
                {
                    AddLoggingEvent(state, loggingEvent);
                }

                if (state.Buffer.Count > 0)
                {
                    InternalLogger.Debug("Sending residual html email events");
                    SendEvents(state);
                }

                InternalLogger.Debug("HtmlSmtp target worker thread exit");
            }
        }

        public static int ToMillseconds(TimeSpan t)
        {
            var milli = t.Ticks / TimeSpan.TicksPerMillisecond;
            return (milli < int.MaxValue) ? (int)milli : int.MaxValue;
        }

        /// <summary>
        ///     Get the timeout to wait for the next possible logging event, ensuring that
        ///     the time doesn't cause events in the buffer to be sent from being delayed
        ///     once the holddown timeout expires.
        /// </summary>
        private TimeSpan GetWaitTimeOut(WorkerState state)
        {
            DateTime now = DateTime.UtcNow;
            var maxHolddownInterval = TimeSpanUtils.Max(state.LastSend + HolddownPeriod - now, TimeSpan.Zero);
            return new TimeSpan(0, 0, 30);
        }


        private void SendEvents(WorkerState state)
        {
            try
            {
                var now = DateTime.UtcNow;

                long loggingEventsLost = Interlocked.Read(ref _lostEvents);
                Deliver(MakeEmail(state.Buffer.ToList(), loggingEventsLost));

                // Reduce the lost event count by the number that have been reported
                // in the email.
                Interlocked.Add(ref _lostEvents, -loggingEventsLost);
                state.Buffer.Clear();
                state.LastSend = now;
            }
            catch (Exception e)
            {
                // Failed to send the email. The buffer will still hold the messages.
                // Although not ideal, the next email will get larger.
                InternalLogger.Error(e, "Failed to send buffer: {0}", e.Message);
            }
        }

        protected virtual void Deliver(MailMessage mailMessage)
        {
            SmtpClientFactory
                .ParseConfiguration(Transport.Render(new LogEventInfo()))
                .Send(mailMessage);
        }

        private void AddLoggingEvent(WorkerState state, LogEventInfo loggingEvent)
        {
            if (loggingEvent.Level >= _triggerLevel)
            {
                // Add a property for the template so that the transform can 
                // determine which logging events met the triggering criteria.
                loggingEvent.Properties[IsTriggerLoggingEvent] = true;

                // Move the events from the non-triggering event queue to the buffer
                while (state.NonTriggeringEvents.Count > 0)
                {
                    state.Buffer.Enqueue(state.NonTriggeringEvents.Dequeue());
                }
                state.Buffer.Enqueue(loggingEvent);
            }
            else
            {
                // Keep the logging events in a small bounded queue
                while (state.NonTriggeringEvents.Count > 0 &&
                       state.NonTriggeringEvents.Count >= PreTriggerMessages)
                {
                    state.NonTriggeringEvents.Dequeue();
                }
                state.NonTriggeringEvents.Enqueue(loggingEvent);
            }
        }

        public MailMessage MakeEmail(IList<LogEventInfo> buffer, long loggingEventsLost)
        {
            var model = MakeMailBodyModel(buffer, loggingEventsLost, _triggerLevel);
            
            var dummyInfo = new LogEventInfo(LogLevel.Off, "", "");
            dummyInfo.Properties["LostEvents"]= loggingEventsLost;
            dummyInfo.Properties["TriggerEvents"] = buffer.Count(e=>e.Level >= _triggerLevel);
            dummyInfo.Properties["TotalEvents"] = buffer.Count;
            dummyInfo.Properties["GroupDevEvents"] = buffer.Count(e => e.Level < LogLevel.Info);
            dummyInfo.Properties["GroupInfoEvents"] = buffer.Count(e => e.Level == LogLevel.Info);
            dummyInfo.Properties["GroupWarnEvents"] = buffer.Count(e => e.Level == LogLevel.Warn);
            dummyInfo.Properties["GroupAlertEvents"] = buffer.Count(e => e.Level > LogLevel.Warn);


            var mailMessage = new MailMessage
            {
                From = new MailAddress(From.Render(dummyInfo)),
                Subject = Subject.Render(dummyInfo),
            };
            mailMessage.To.Add(To.Render(dummyInfo));
            mailMessage.BodyEncoding = Encoding.ASCII;
            var htmlView = AlternateView.CreateAlternateViewFromString(
                RenderHtmlEmailBody(model),
                new ContentType("text/html;charset=utf-8"));

            // Add icons as attachments to the email.
            LinkedResources.AddAttachmentIffGifFound(htmlView, "fatal", "/image/FatalIcon.gif");
            LinkedResources.AddAttachmentIffGifFound(htmlView, "error", "/image/ErrorIcon.gif");
            LinkedResources.AddAttachmentIffGifFound(htmlView, "warn", "/image/WarnIcon.gif");
            LinkedResources.AddAttachmentIffGifFound(htmlView, "info", "/image/InfoIcon.gif");
            LinkedResources.AddAttachmentIffGifFound(htmlView, "debug", "/image/DebugIcon.gif");
            LinkedResources.AddAttachmentIffGifFound(htmlView, "trace", "/image/TraceIcon.gif");

            mailMessage.AlternateViews.Add(htmlView);
            if (ReplyTo != null)
            {
                var replyTo = ReplyTo.Render(dummyInfo);
                if (!string.IsNullOrEmpty(replyTo))
                {
                    mailMessage.ReplyToList.Add(new MailAddress(replyTo));
                }
            }
            return mailMessage;
        }

        public string RenderHtmlEmailBody(object model)
        {
            return _mailTemplate(model);
        }


        public static Func<object, string> MakeTemplate()
        {
            return Handlebars.Compile(Resources.Email);
        }


        public static object MakeMailBodyModel(
            IList<LogEventInfo> buffer,
            long loggingEventsLost,
            LogLevel triggerLevel)
        {
            var model = new
            {
                TriggerEvents = buffer.Where(e => e.Level >= triggerLevel),
                AllEvents = buffer,
                EventsLost = loggingEventsLost,
            };
            return model;
        }
    }
}