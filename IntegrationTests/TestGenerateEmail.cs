/*
 *   (c) Greg Brackley, 2010
 *   
 *   $Id: TestGenerateEmail.cs 13 2010-10-06 01:01:29Z greg $ 
 */

using System;
using NLog.Common;
using NLog.HtmlSmtpTarget.Utils;
using Xunit;

namespace NLog.HtmlSmtpTarget
{
    public class TestGenerateEmail : IDisposable
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public TestGenerateEmail()
        {
            LogManager.ThrowExceptions = true;
            LogManager.ThrowConfigExceptions = true;

            InternalLogger.LogToConsole = true;
            InternalLogger.LogLevel = LogLevel.Trace;

            var target = new Target.HtmlSmtpTarget
            {
                Name = "TestInstance",
                To = "Greg <greg-nlog-htmlsmtpappender-test@lucidsolutions.co.nz>",
                From = "greg-nlog-htmlsmtpappender-test-nunit@lucidsolutions.co.nz",
                Transport = "smtp://smtp.lucidsolutions.co.nz",
                Subject = "[Fact] %events{triggering} of %events{total} [%events{class.unrecoverable},%events{class.recoverable},%events{class.information},%events{class.debug}] (lost %events{lost})",
            };

            LogManager.Configuration.AddTarget("HtmlSmtp", target);

        }


        public void Dispose()
        {
            LogManager.Configuration.RemoveTarget("HtmlSmtp");
        }

        [Fact]
        public void TestLogMessage()
        {
            NestedDiagnosticsContext.Push("TestNdc");
            Log.Error("This is a test message");
        }


        [Fact]
        public void TestMultiLogMessage()
        {
            NestedDiagnosticsContext.Push("TestNdc");
            for (int i = 0; i < 20; ++i)
            {
                Log.DebugFormat("debug message {0}", i);
            }
            Log.Trace("Trace message");
            Log.Debug("Debug message");
            Log.Info("Info message");
            Log.Info("Info message");
            Log.Info("Info message");
            Log.Info("This is a\nmultiline message\nthat should have\nhtml breaks");
            Log.Info("A message with <html> elements that should be rendered as text");
            Log.Warn(new Exception(), "Warning message (this should be a triggering message)");
                          
            Log.Info("Info message");
            Log.Error("Error message 1 (a non-trigger error, due to holddown after last warning)");
            Log.InfoFormat("Info message");
            Log.Error("Error message 2");
            Log.InfoFormat("Info message");
            Log.Trace("Trace message");
            Log.Debug("Debug message");
            Log.Info("Info message");
            Log.Warn("Warn message");
            Log.Error("Error message");
            Log.Fatal("Fatal message");
            Log.Error(new Exception("Test"), "Error message 3 (second triggering  due to appender being closed)");
        }
    }
}
