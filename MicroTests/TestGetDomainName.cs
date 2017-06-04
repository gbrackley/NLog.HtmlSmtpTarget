/*
 *   (c) Greg Brackley, 2010
 *   
 *   $Id: TestGetDomainName.cs 8 2010-09-30 01:59:10Z greg $ 
 */

using NLog.HtmlSmtpTarget.Target.Utils;
using Xunit;
using XunitShould;

namespace NLog.HtmlSmtpTarget
{
    public class TestGetDomainName
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();


        [Fact]
        public void TestParseDomainNameHostNameOnly()
        {
            "".ShouldEqual(DnsUtils.GetDomainName("bob"));
        }

        [Fact]
        public void TestParseDomainNameHostNameWithTrailingDot()
        {
            "".ShouldEqual(DnsUtils.GetDomainName("bart."));
        }

        [Fact]
        public void TestParseDomainNameFullyQualifiedDomainName()
        {
            "simpson.com".ShouldEqual(DnsUtils.GetDomainName("homer.simpson.com"));
        }

        [Fact]
        public void TestParseLocalName()
        {
            "local".ShouldEqual(DnsUtils.GetDomainName("host.local"));
        }


        /// <summary>
        ///     The machine running the test <b>must</b> have a DNS domain name configured for this
        ///     test to complete successfully.
        /// </summary>
        [Fact]
        public void TestParseLocalDomainName()
        {
            var domainName = DnsUtils.GetDomainName();
            Log.Info("domain is '{0}'", domainName);

            domainName.ShouldNotBeEmpty();
        }
    }
}