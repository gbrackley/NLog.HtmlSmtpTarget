using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text.RegularExpressions;
using NLog.Common;
using NLog.HtmlSmtpTarget.Target;
using NLog.HtmlSmtpTarget.Target.Utils;

namespace NLog.HtmlSmtpTarget.Formatter
{
    /// <summary>
    /// </summary>
    /// <seealso cref="IStringFormatter" />
    /// <seealso cref="StringFormatter" />
    public static class StringFormatterUtil
    {/*
        public static string Format(
            string format,
            IHtmlSmtpTargetConfig targetConfig,
            IEnumerable<LogEventInfo> events,
            long loggingEventsLost)
        {
            return new StringFormatter().Format(
                format,
                (name, parameters) =>
                {
                    if ("env" == name)
                    {
                        return Environment(parameters);
                    }
                    else if ("events" == name)
                    {
                        return EventCount(parameters, targetConfig, events, loggingEventsLost);
                    }
                    else
                    {
                        InternalLogger.Error(
                                "The replacement parameter '{0}' with parameters '{1}' is not supported",
                                name,
                                parameters);
                        return "";
                    }
                });
        }

        /// <summary>
        ///     Get an environment variable.
        /// </summary>
        public static string Environment(string parameters)
        {
            try
            {
                if (!string.IsNullOrEmpty(parameters))
                {
                    // Lookup the environment variable
                    return System.Environment.GetEnvironmentVariable(parameters);
                }
            }
            catch (SecurityException secEx)
            {
                // This security exception will occur if the caller does not have 
                // unrestricted environment permission. If this occurs the expansion 
                // will be skipped with the following warning message.
                InternalLogger.Debug(
                    secEx,
                    "EnvironmentPatternConverter: Security exception while trying to expand environment variables. Error Ignored. No Expansion.");
            }
            catch (Exception ex)
            {
                InternalLogger.Error(
                    ex,
                    "EnvironmentPatternConverter: Error occurred while converting environment variable.");
            }
            return "";
        }

        /// <summary>
        ///     Given a set of events, provide the value of 'parameters'
        /// </summary>
        public static string EventCount(
            string parameters,
            IHtmlSmtpTargetConfig targetConfig,
            IEnumerable<LogEventInfo> events,
            long loggingEventsLost)
        {
            if (string.IsNullOrEmpty(parameters) ||
                "total".Equals(parameters, StringComparison.InvariantCultureIgnoreCase))
            {
                return events.LongCount().ToString();
            }
            else if ("triggering".Equals(parameters, StringComparison.InvariantCultureIgnoreCase))
            {
                return events
                    .Where(e => e.Level >= targetConfig.TriggerLevel)
                    .LongCount()
                    .ToString();
            }
            else if ("lost".Equals(parameters, StringComparison.InvariantCultureIgnoreCase))
            {
                return loggingEventsLost.ToString();
            }
            else if ("nontriggering".Equals(parameters, StringComparison.InvariantCultureIgnoreCase))
            {
                return events
                    .Where(e => e.Level < targetConfig.TriggerLevel)
                    .LongCount()
                    .ToString();
            }
            else if (Regex.IsMatch(parameters, @"\d+"))
            {
                int level = int.Parse(parameters);
                return events.Where(e => e.Level.Ordinal == level).LongCount().ToString();
            }
            else if (Enumerable.Any<LogLevel>(
                LevelUtils.All,
                e => e.Name.Equals(parameters, StringComparison.InvariantCultureIgnoreCase)))
            {
                return events
                    .Where(e => e.Level.Name.Equals(parameters, StringComparison.InvariantCultureIgnoreCase))
                    .LongCount()
                    .ToString();
            }
            else if ("class.unrecoverable".Equals(parameters, StringComparison.InvariantCultureIgnoreCase))
            {
                // Critical < level <= Off
                return events.Where(e => e.Level > LogLevel.Fatal).LongCount().ToString();
            }
            else if ("class.recoverable".Equals(parameters, StringComparison.InvariantCultureIgnoreCase))
            {
                // Error < level <= Critical
                return events
                    .Where(e => e.Level > LogLevel.Error && e.Level <= LogLevel.Fatal)
                    .LongCount()
                    .ToString();
            }
            else if ("class.information".Equals(parameters, StringComparison.InvariantCultureIgnoreCase))
            {
                // Debug < level <= Notice
                return events
                    .Where(e => e.Level > LogLevel.Debug && e.Level <= LogLevel.Info)
                    .LongCount()
                    .ToString();
            }
            else if ("class.debug".Equals(parameters, StringComparison.InvariantCultureIgnoreCase))
            {
                // level <= Debug
                return events.Where(e => e.Level <= LogLevel.Debug).LongCount().ToString();
            }
            else
            {
                InternalLogger.Error("Invalid event count parameter '{0}'", parameters);
                return "";
            }
        }*/
    }
}
