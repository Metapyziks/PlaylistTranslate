using System;

namespace PlaylistTranslate
{
    /// <summary>
    /// Contains approximate string matching
    /// </summary>
    public static class LevenshteinDistance
    {
        /// <summary>
        /// Compute the distance between two strings.
        /// </summary>
        public static int Levenshtein(this string s, string t)
        {
            var n = s.Length;
            var m = t.Length;

            if (n == 0) return m;
            if (m == 0) return n;

            var d = new int[n + 1, m + 1];

            for (var i = 0; i <= n; d[i, 0] = i++) {}
            for (var j = 0; j <= m; d[0, j] = j++) {}

            for (var i = 1; i <= n; i++) {
                for (var j = 1; j <= m; j++) {
                    var cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[n, m];
        }
    }
}