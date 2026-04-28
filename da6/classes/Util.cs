using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace da6
{
    public class Util
    {
        /// <summary>
        /// translate a php regex pattern to c# format
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        internal static Regex ToDotNetRegex(string pattern)
        {
            if (pattern == null)
                return null;

            if (!pattern.StartsWith("%"))
                return new Regex(pattern);

            int last = pattern.LastIndexOf("%");
            string pat = pattern.Substring(1, last - 1);

            var opt = pattern.Substring(last).TrimStart('%');
            if (opt.Length > 0)
            {
                pat = $"(?{opt}){pat}"; // todo check each character and make sure it's valid
            }

            return new Regex(pat);
        }

        internal static void EnsureDirectory(string targetPath)
        {
            if (!string.IsNullOrWhiteSpace(targetPath) && !Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }
        }
    }
}
