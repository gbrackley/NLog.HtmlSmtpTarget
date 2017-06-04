/*
 *   (c) Greg Brackley, 2010
 *   
 *   $Id: TestXmlConfigure.cs 12 2010-10-05 09:54:31Z greg $ 
 */

using System.IO;
using System.Xml;
using NLog.Common;
using NLog.Config;
using Xunit;
using XunitShould;

namespace NLog.HtmlSmtpTarget
{
    public class TestXmlConfigure
    {
        public TestXmlConfigure()
        {
            LogManager.ThrowExceptions = true;
            LogManager.ThrowConfigExceptions = true;

            InternalLogger.LogToConsole = true;
            InternalLogger.LogLevel = LogLevel.Trace;
        }


        [Fact]
        public void TestLoadEmptyConfiguration()
        {
            var configStr = @"<?xml version=""1.0"" encoding=""iso-8859-1""?>
          <nlog
            throwExceptions=""true""
            xmlns = ""http://www.nlog-project.org/schemas/NLog.xsd""
            xmlns:xsi = ""http://www.w3.org/2001/XMLSchema-instance"" >
 
             <targets>
             </targets>

               <rules>
                </rules>
            </nlog>";


            var config = new XmlLoggingConfiguration(
                XmlReader.Create(new StringReader(configStr)),
                "TestCaseNonXmlFile.config");

            config.ShouldNotBeNull();
        }

        [Fact]
        public void TestLoadConfiguration()
        {
            var configStr = @"<?xml version=""1.0"" encoding=""iso-8859-1""?>
          <nlog
            throwExceptions=""true""
            xmlns = ""http://www.nlog-project.org/schemas/NLog.xsd""
            xmlns:xsi = ""http://www.w3.org/2001/XMLSchema-instance"" >

             <extensions> 
                <add assembly=""HtmlSmtpTarget""/> 
             </extensions>

             <targets>
               <default-target-parameters 
                    xsi:type=""HtmlSmtp""
                    transport=""smtp:localhost"" />


                <target
                    name=""HtmlSmtp""
                    xsi:type=""HtmlSmtp""
                    transport=""smtp:localhost""
                    to=""sample@example.com""
                    from=""sample@example.com""
                    replyto=""sample@example.com""
                    subject=""a sample layout""
                    holddownperiod=""0:15:0""
                    eventbacklog=""50""
                    maximumeventspermessage=""100""
                    pretriggermessages=""10""
                    triggerlevel=""WARN""
                 />
              </targets>

               <rules>
                    <logger name=""*"" minlevel=""Trace"" writeTo=""HtmlSmtp"" />
                </rules>
            </nlog >";


            var config = new XmlLoggingConfiguration(
                XmlReader.Create(new StringReader(configStr)),
                "TestCaseNonXmlFile.config");

            config.ShouldNotBeNull();
        }

     
    }
}