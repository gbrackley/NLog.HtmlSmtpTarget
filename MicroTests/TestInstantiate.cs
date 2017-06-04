/*
 *   (c) Greg Brackley, 2010
 *   
 *   $Id: TestInstantiate.cs 7 2010-09-29 05:55:12Z greg $ 
 */

using NLog.Config;
using Xunit;
using XunitShould;

namespace NLog.HtmlSmtpTarget
{
    public class TestInstantiate
    {
        [Fact]
        public void TestInstantiateAppender()
        {
            var target = new Target.HtmlSmtpTarget();
            target.ShouldNotBeNull();
        }


        [Fact]
        public void TestInstantiateAndActicvateAppenderSampleManditoryFields()
        {
            var target = new Target.HtmlSmtpTarget()
            {
                To = "homer@simpson.com",
                From = "homer@simpson.com",
            };

            LoggingConfiguration config = new LoggingConfiguration();
            config.AddTarget("test", target);
            config.Install(new InstallationContext());

            target.ShouldNotBeNull();
        }
    }
}