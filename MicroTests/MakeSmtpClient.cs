using System;
using NLog.HtmlSmtpTarget.Target.HtmlSmtp;
using Xunit;
using XunitShould;

namespace NLog.HtmlSmtpTarget
{
    public class MakeSmtpClient
    {
        [Fact]
        public void MakeDefaultTransportString()
        {
            var configStr = SmtpClientFactory.MakeDefaultTransport();

            configStr.ShouldNotBeNull();
            configStr.ShouldContain("smtp", StringComparison.InvariantCultureIgnoreCase);
        }


        [Theory,
         InlineData("smtp://localhost"),
         InlineData("smtp://localhost:25"),
         InlineData("smtp://localhost:submission"),
         InlineData("smtp://localhost:587"),
         InlineData("smtps://smtp.example.com"),
        ]
        public void SmtpClient(string transportConfig)
        {
            var client = SmtpClientFactory.ParseConfiguration(transportConfig);
            client.ShouldNotBeNull();
        }
    }
}