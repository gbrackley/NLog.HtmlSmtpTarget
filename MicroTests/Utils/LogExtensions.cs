/*
 *   (c) Greg Brackley, 2010
 *   
 *   $Id: LogExtensions.cs 12 2010-10-05 09:54:31Z greg $ 
 */

using System;
using JetBrains.Annotations;

namespace NLog.HtmlSmtpTarget.Utils
{
    public static class LogExtensions
    {
        [StringFormatMethod("format")]
        public static void TraceFormat(this ILogger logger, string format, params object[] args)
        {
            logger.Trace((Exception) null, format, args);
        }

        [StringFormatMethod("format")]
        public static void DebugFormat(this ILogger logger, string format, params object[] args)
        {
            logger.Debug((Exception) null, format, args);
        }

        [StringFormatMethod("format")]
        public static void InfoFormat(this ILogger logger, string format, params object[] args)
        {
            logger.Info((Exception) null, format, args);
        }
    }
}