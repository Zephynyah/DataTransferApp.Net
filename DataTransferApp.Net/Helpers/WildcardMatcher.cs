using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DataTransferApp.Net.Helpers
{
    public static class WildcardMatcher
    {
        // Converts a wildcard pattern (e.g., "New folder*") to a regex
        public static Regex WildcardToRegex(string pattern)
        {
            return new Regex("^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$", RegexOptions.IgnoreCase);
        }

        // Returns true if the input matches any of the wildcard patterns
        public static bool IsMatch(string input, IEnumerable<string> patterns)
        {
            foreach (var pattern in patterns)
            {
                if (WildcardToRegex(pattern).IsMatch(input))
                    return true;
            }
            return false;
        }
    }
}
