﻿/*
 *   (c) Greg Brackley, 2010
 *   
 *   $Id: SmtpClientFactory.cs 12 2010-10-05 09:54:31Z greg $ 
 */

using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using NLog.Common;
using NLog.HtmlSmtpTarget.Target.Utils;

namespace NLog.HtmlSmtpTarget.Target.HtmlSmtp
{
    public static class SmtpClientFactory
    {
        public static string MakeDefaultTransport()
        {
            var env = Environment.GetEnvironmentVariable("NLOG_SMTP_CONFIG");
            if (!string.IsNullOrEmpty(env))
            {
                InternalLogger.Debug("Using environment based configuration '{0}", env);
                return env;
            }
            else
            {
                var domain = DnsUtils.GetDomainName();
                if (!string.IsNullOrEmpty(domain))
                {
                    var mxs = DnsQuery.QueryMx(domain);
                    var mxRecord = mxs
                        .OrderBy(mx => mx.Preference)
                        .FirstOrDefault();
                    if (mxRecord != null && !string.IsNullOrEmpty(mxRecord.Name))
                    {
                        return string.Format("smtp://{0}", mxRecord.Name);
                    }
                    else
                    {
                        InternalLogger.Debug(
                            "The domain '{0}' has no MX record, using localhost as the SMTP smart host", domain);
                        return "smtp://localhost";
                    }
                }
                else
                {
                    InternalLogger.Debug(
                        "The machine has no domain name (primary DNS suffix), using localhost as the SMTP smart host");
                    return "smtp://localhost";
                }
            }
        }

        /// <summary>
        ///     A regular expression to extract the SMTP settings from an url.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Support two different urls
        ///         <ul>
        ///             <li>An url with a 'file' scheme for a maildrop configuration</li>
        ///             <li>An url with a 'smtp[s]' scheme for smtp transport</li>
        ///         </ul>
        ///     </para>
        /// </remarks>
        private const string ConfigUrlMatch =
            @"^ # start of input

                #
                #  'file' scheme has a optional path 
                # 
                ( 
                    # mandatory scheme 'file:'
                    (?<scheme> file ) : (?<path> .* )
                )
                |

                ( 
                    # scheme with a colon suffix. The scheme is optional so that
                    # a simple configuration with just a host name is supported.
                    ( (?<scheme> smtp | smtps ) : )?

                    # The authority of the form: 
                    #   '//' [ username [ ':' password ] '@' ] hostname|ipv4|ipv6 ':' port|service
                    ( //
                        # The username and an optional password
                        (   (?<username> [^/?\#:@]* )
                            ( : (?<password> [^/?\#:@]* ) )?
                            @
                        )?

                        (?<host> [^/?\#:]* )

                        ( : (?<service> \d+ | [a-zA-Z]\w+ ) )?   # optional service (numeric or word)
                    )?

                    (?<path> [^?\#]* )                   # path

                    (   ( \? | & )                       # question mark prefix
                        (?<query> [^\#&]*)               # first query parameter
                        ( & (?<query2> [^\#&]* ) )?      # subsequent query parameters
                    )?
                 ) # end of smtp/smtps scheme
             $";

        public static Match MatchConfig(string configuration)
        {
            return Regex.Match(
                configuration,
                ConfigUrlMatch,
                RegexOptions.Compiled |
                RegexOptions.Singleline |
                RegexOptions.IgnorePatternWhitespace |
                RegexOptions.CultureInvariant);
        }

        /// <summary>
        ///     Construct an SMTP client given a single configuration string.
        /// </summary>
        /// <remarks>
        ///     The <see cref="SmtpClient" /> implementation supports STARTTLS. The SmtpClient doesn't
        ///     support SMTP over TLS/SLL (port 465).
        /// </remarks>
        public static SmtpClient ParseConfiguration(string smtpConfiguration)
        {
            Match match = MatchConfig(smtpConfiguration);
            if (match.Success)
            {
                string scheme = match.GetSingletonCapture("scheme");
                if ("file".Equals(scheme, StringComparison.InvariantCultureIgnoreCase))
                {
                    return new SmtpClient
                    {
                        DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
                        PickupDirectoryLocation = match.GetSingletonCapture("path"),
                    };
                }
                else if ("smtp".Equals(scheme, StringComparison.InvariantCultureIgnoreCase) ||
                         "smtps".Equals(scheme, StringComparison.InvariantCultureIgnoreCase))
                {
                    var enableSsl = "smtps".Equals(scheme, StringComparison.InvariantCultureIgnoreCase);

                    int port;

                    var service = match.GetSingletonCapture("service");
                    if (!string.IsNullOrEmpty(service))
                    {
                        if ("smtp".Equals(service, StringComparison.InvariantCultureIgnoreCase))
                        {
                            port = 25;
                        }
                        else if ("submission".Equals(service, StringComparison.InvariantCultureIgnoreCase))
                        {
                            port = 587;
                        }
                        else if (int.TryParse(service, out port))
                        {
                            // ok
                        }
                        else
                        {
                            InternalLogger.Warn("Failed to parse service/port '{0}'", service);
                            port = 25;
                        }
                    }
                    else
                    {
                        port = 25;
                    }

                    var host = match.GetSingletonCapture("host");
                    var client = new SmtpClient
                    {
                        Host = host ?? "localhost",
                        Port = port,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        EnableSsl = enableSsl,
                        Credentials = MakeCredentials(
                            match.GetSingletonCapture("username"),
                            match.GetSingletonCapture("password")),
                    };
                    return client;
                }
            }
            else
            {
                throw new ArgumentException(
                    string.Format("SMTP configuration '{0}' is no valid", smtpConfiguration), "smtpConfiguration");
            }
            throw new ArgumentException(
                string.Format("SMTP configuration '{0}' is no valid", smtpConfiguration), "smtpConfiguration");
        }

        private static ICredentialsByHost MakeCredentials(
            string username,
            string password)
        {
            if (!string.IsNullOrEmpty(username))
            {
                // Normal SMTP authentication
                return new NetworkCredential(username, password);
            }
            else
            {
                // No SMTP authentication
                return null;
            }
        }
    }
}