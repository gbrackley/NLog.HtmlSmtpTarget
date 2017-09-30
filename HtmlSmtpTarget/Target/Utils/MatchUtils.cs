/*
 *   (c) Greg Brackley, 2010
 *   
 *   $Id: MatchUtils.cs 12 2010-10-05 09:54:31Z greg $ 
 */

using System.Text.RegularExpressions;

namespace NLog.HtmlSmtpTarget.Target.Utils
{
    /// <summary>
    ///     Utilites in support of <see cref="Match" />
    /// </summary>
    public static class MatchUtils
    {
        public static string GetSingletonCapture(this Match match, string groupName)
        {
            return match
                .Groups[groupName]
                .Captures
                .GetSingleton<Capture>(null)
                ?.Value;
        }

        public static string GetSingletonOrDefaultCapture(this Match match, string groupName)
        {
            return match
                .Groups[groupName]
                .Captures
                .GetSingletonOrDefault<Capture>()
                ?.Value;
        }
    }
}