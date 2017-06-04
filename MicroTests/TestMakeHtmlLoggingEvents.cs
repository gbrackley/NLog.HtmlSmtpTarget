/*
 *   (c) Greg Brackley, 2010
 *   
 *   $Id: TestMakeXmlLoggingEvents.cs 8 2010-09-30 01:59:10Z greg $ 
 */

using NLog.HtmlSmtpTarget.Utils;
using System;
using System.Globalization;
using Xunit;
using XunitShould;

namespace NLog.HtmlSmtpTarget
{
    public class TestMakeHtmlLoggingEvents
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        [Fact]
        public void TestCreateSingleEventBody()
        {
            const string sampleMessage = "This is a sample message";
            var events = new[]
            {
                new LogEventInfo(LogLevel.Fatal, "MyTestLogger.example.com", sampleMessage)
            };

            // Make the email
            var body = Target.HtmlSmtpTarget.RenderHtmlEmailBody(Target.HtmlSmtpTarget.MakeEmailBodyModel(events, 0));
            Log.InfoFormat("Mail body is:\n{0}", body);


            body.ShouldContain("<head>");
            body.ShouldContain("<body>");
            body.ShouldContain("<html>");
            body.ShouldContain(sampleMessage);
        }


        [Fact]
        public void TestCreateMultipleEventBody()
        {
            const string sampleMessage1 = "111 This is a sample message";
            const string sampleMessage2 = "222 This is a sample message";
            var events = new[]
            {
                new LogEventInfo(LogLevel.Fatal, "MyTestLogger.example.com", sampleMessage1),
                new LogEventInfo(LogLevel.Info, "MyTestLogger.example.com", sampleMessage2)
            };

            // Make the email
            var body = Target.HtmlSmtpTarget.RenderHtmlEmailBody(Target.HtmlSmtpTarget.MakeEmailBodyModel(events, 0));
            Log.InfoFormat("Mail body is:\n{0}", body);


            body.ShouldContain(sampleMessage1);
            body.ShouldContain(sampleMessage2);
        }

        [Fact]
        public void TestCreateSingleEventWithException()
        {
            const string sampleMessage = "This is a sample message";
            var sampleExceptionMessage = "Sample exception message";
            var events = new[]
            {
                new LogEventInfo(LogLevel.Fatal, "MyTestLogger.example.com", CultureInfo.InvariantCulture,
                    sampleMessage, new object[] { }, new NotSupportedException(sampleExceptionMessage))
            };

            // Make the email
            var body = Target.HtmlSmtpTarget.RenderHtmlEmailBody(Target.HtmlSmtpTarget.MakeEmailBodyModel(events, 0));
            Log.InfoFormat("Mail body is:\n{0}", body);


            body.ShouldContain(sampleExceptionMessage);
        }
    }
}