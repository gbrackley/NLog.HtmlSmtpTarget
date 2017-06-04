/*
 *   (c) Greg Brackley, 2010
 *   
 *   $Id: TestQueryMxRecords.cs 9 2010-09-30 02:00:16Z greg $ 
 */

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NLog.HtmlSmtpTarget.Target.Utils;
using Xunit;
using XunitShould;

namespace NLog.HtmlSmtpTarget
{
    /// <summary>
    ///     These tests perform basic exercise tests. The results from public
    ///     domains (like google) are likely to return geographic based results,
    ///     so these tests aren't very restrictive.
    /// </summary>
    public class TestQueryMxRecords
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();


        [Fact]
        public void TestQueryMxGoogle()
        {
            const string domain = "google.com";
            Log.Info("Query domain '{0}'", domain);
            var mailExchangers = DnsQuery.QueryMx(domain);
            LogMxs(domain, mailExchangers);

            mailExchangers.ShouldNotBeNull();
            5.ShouldEqual(mailExchangers.Count());
        }

        [Fact]
        public void TestQueryMxTrademe()
        {
            const string domain = "trademe.co.nz";
            Log.Info("Query domain '{0}'", domain);
            var mailExchangers = DnsQuery.QueryMx(domain);
            LogMxs(domain, mailExchangers);

            mailExchangers.ShouldNotBeNull();
            mailExchangers.ShouldNotBeEmpty();
            mailExchangers.Count().ShouldBeGreaterThanOrEqualTo(3);
        }

        [Fact]
        public void TestQueryMxSlashdot()
        {
            const string domain = "slashdot.org";
            Log.Info("Query domain '{0}'", domain);
            var mailExchangers = DnsQuery.QueryMx(domain);
            LogMxs(domain, mailExchangers);

            mailExchangers.ShouldNotBeNull();
            mailExchangers.Count().ShouldEqual(1);
        }

        [Fact]
        //[ExpectedException(typeof(Win32Exception), ExpectedMessage="DNS name does not exist")]
        public void TestQueryInvalidDomain()
        {
            Assert.Throws<Win32Exception>(() =>
            {
                const string domain = "thisdomaindoesnotexist123456.net";
                DnsQuery.QueryMx(domain);
            });
        }


        private static void LogMxs(string domain, IEnumerable<MxRecord> mailExchangers)
        {
            Log.Info("{0} mail exchanger(s) for domain '{1}'", mailExchangers.Count(), domain);
            foreach (var mailExchanger in mailExchangers)
            {
                Log.Info(" MX {0} [{1}]", mailExchanger.Name, mailExchanger.Preference);
            }
        }
    }
}