using System;

namespace MediatrExtension.Core.Extensions
{
    public static class StringExtensions
    {
        public static string TrimPrefix(this string s, string prefix, StringComparison stringComparision = StringComparison.OrdinalIgnoreCase)
        {
            if (s.StartsWith(prefix, stringComparision))
            {
                return s.Substring(prefix.Length);
            }

            return s;
        }

        public static string TrimSuffix(this string s, string suffix, StringComparison stringComparision = StringComparison.OrdinalIgnoreCase)
        {
            if (s.EndsWith(suffix, stringComparision))
            {
                return s.Substring(0, s.Length - suffix.Length);
            }

            return s;
        }
    }
}
