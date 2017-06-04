/*
 *   (c) Greg Brackley, 2010
 *   
 *   $Id: TestStringFormatter.cs 12 2010-10-05 09:54:31Z greg $ 
 */

using System;
using NLog.HtmlSmtpTarget.Formatter;
using Xunit;
using XunitShould;

namespace NLog.HtmlSmtpTarget
{
    public class TestStringFormatter
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();


        [Fact]
        public void TestEmpty()
        {
            var stringFormatter = new StringFormatter();
            var str = stringFormatter.Format("", (name, parameters) => "");

            "".ShouldEqual(str);
        }

        [Fact]
        public void TestEmptyConstant()
        {
            var stringFormatter = new StringFormatter();
            var str = stringFormatter.Format("This is a test", (name, parameters) => "");

            "This is a test".ShouldEqual(str);
        }

        [Fact]
        public void TestPercentage()
        {
            var stringFormatter = new StringFormatter();
            var str = stringFormatter.Format("%%", (name, parameters) => "");

            "%".ShouldEqual(str);
        }

        [Fact]
        public void TestEmbeddedPercentage()
        {
            var stringFormatter = new StringFormatter();
            var str = stringFormatter.Format("GST is 15%%", (name, parameters) => "");

            "GST is 15%".ShouldEqual(str);
        }

        [Fact]
        public void TestSingleNoParameters()
        {
            var stringFormatter = new StringFormatter();
            var str = stringFormatter.Format(
                "Up is %UP",
                (name, parameters) =>
                {
                    if (name == "UP")
                    {
                        parameters.ShouldBeNull();
                        return "down";
                    }
                    throw new ArgumentException(
                        string.Format("The replacement parameter '{0}' is not supported", name));
                });

            "Up is down".ShouldEqual(str);
        }

        [Fact]
//        [ExpectedException(typeof (ArgumentException))]
        public void TestSingleInavalidName()
        {
            var stringFormatter = new StringFormatter();
            Assert.Throws<ArgumentException>(() =>
            {
                stringFormatter.Format(
                    "Up is %InvalidName",
                    (name, parameters) =>
                    {
                        throw new ArgumentException(
                            string.Format("The replacement parameter '{0}' is not supported", name));
                    });
            });
        }

        /// <summary>
        ///     Replacement parameters that don't match pass through without change
        /// </summary>
        [Fact]
        public void TestNoAValidName()
        {
            var stringFormatter = new StringFormatter();
            var str = stringFormatter.Format(
                "Name is %#",
                (name, parameters) =>
                {
                    throw new ArgumentException(
                        string.Format("The replacement parameter '{0}' is not supported", name));
                });
            Log.Info(str);
            "Name is %#".ShouldEqual(str);
        }


        [Fact]
        public void TestSingleWithParameter()
        {
            var stringFormatter = new StringFormatter();
            var str = stringFormatter.Format(
                "Up is %GO{left}",
                (name, parameters) =>
                {
                    if (name == "GO")
                    {
                        parameters.ShouldNotBeNull();
                        return parameters;
                    }
                    throw new ArgumentException(
                        string.Format("The replacement parameter '{0}' is not supported", name));
                });

            "Up is left".ShouldEqual(str);
        }

        [Fact]
        public void TestSingleWithParameters()
        {
            var stringFormatter = new StringFormatter();
            var str = stringFormatter.Format(
                "Up is %GO{left}, count is %count, size is %size{}",
                (name, parameters) =>
                {
                    if (name == "GO")
                    {
                        parameters.ShouldNotBeNull();
                        return parameters;
                    }
                    else if (name == "count")
                    {
                        return "23";
                    }
                    else if (name == "size")
                    {
                        return "99";
                    }
                    throw new ArgumentException(
                        string.Format("The replacement parameter '{0}' is not supported", name));
                });

            "Up is left, count is 23, size is 99".ShouldEqual(str);
        }
    }
}