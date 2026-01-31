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
            // Escape the pattern and replace wildcards with safer regex patterns
            string escaped = Regex.Escape(pattern);

            // Replace escaped asterisks with [^/]* to avoid catastrophic backtracking
            // This limits the wildcard to not match path separators
            escaped = escaped.Replace("\\*", "[^/]*");

            // Replace escaped question marks with [^/] to match any single character except path separator
            escaped = escaped.Replace("\\?", "[^/]");

            return new Regex("^" + escaped + "$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.NonBacktracking, TimeSpan.FromSeconds(5));
        }

        // Returns true if the input matches any of the wildcard patterns
        public static bool IsMatch(string input, IEnumerable<string> patterns)
        {
            return patterns.Any(pattern => WildcardToRegex(pattern).IsMatch(input));
        }
    }
}