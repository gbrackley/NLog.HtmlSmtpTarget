/*
 *   (c) Greg Brackley, 2010
 *   
 *   $Id: TestSmtpUrlConfiguration.cs 12 2010-10-05 09:54:31Z greg $ 
 */

using NLog.HtmlSmtpTarget.Target.HtmlSmtp;
using NLog.HtmlSmtpTarget.Target.Utils;
using Xunit;
using XunitShould;

namespace NLog.HtmlSmtpTarget
{
    /// <summary>
    /// </summary>
    /// <seealso cref="HtmlSmtp.ConfigUrlMatch" />
    public class TestSmtpUrlConfiguration
    {
        [Fact]
        public void TestEmpty()
        {
            var match = SmtpClientFactory.MatchConfig("");

            match.ShouldNotBeNull();
            match.Success.ShouldBeTrue();
        }

        [Fact]
        public void TestSimpleFile()
        {
            var match = SmtpClientFactory.MatchConfig("file:c:\\maildrop\\");

            match.Success.ShouldBeTrue();
            "file".ShouldEqual(match.GetSingletonCapture("scheme"));
            @"c:\maildrop\".ShouldEqual(match.GetSingletonCapture("path"));
        }

        [Fact]
        public void TestSimpleFilePath()
        {
            var match = SmtpClientFactory.MatchConfig("file:/my/mydrop/queue");
            match.Success.ShouldBeTrue();
            "file".ShouldEqual(match.GetSingletonCapture("scheme"));
        }

        [Fact]
        public void TestSimpleSmtp()
        {
            var match = SmtpClientFactory.MatchConfig("smtp://smtp.domain.com");
            match.Success.ShouldBeTrue();
            "smtp".ShouldEqual(match.GetSingletonCapture("scheme"));
        }

        [Fact]
        public void TestSimpleSmtps()
        {
            var match = SmtpClientFactory.MatchConfig("smtps://smtp.domain.com");
            match.Success.ShouldBeTrue();
            "smtps".ShouldEqual(match.GetSingletonCapture("scheme"));
        }


        [Fact]
        public void TestSmtpWithQueryParameters()
        {
            var match = SmtpClientFactory.MatchConfig("smtp://uuuu:pppp@smtp.domain.com:smtp?authentication=ntlm");
            match.Success.ShouldBeTrue();
            "smtp".ShouldEqual(match.GetSingletonCapture("scheme"));
            "smtp.domain.com".ShouldEqual(match.GetSingletonCapture("host"));
            "smtp".ShouldEqual(match.GetSingletonCapture("service"));
            "uuuu".ShouldEqual(match.GetSingletonCapture("username"));
            "pppp".ShouldEqual(match.GetSingletonCapture("password"));
        }
    }
}