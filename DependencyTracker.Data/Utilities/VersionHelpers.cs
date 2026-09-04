using System;
using System.Collections.Generic;

namespace DependencyTracker.Data.Utilities
{
    /// <summary>
    /// Dotted-numeric version comparison helpers shared by application-version
    /// grouping and the DLL search. Segments are compared numerically; missing
    /// segments count as zero (13.0 == 13.0.0).
    /// </summary>
    public static class VersionHelpers
    {
        /// <summary>
        /// Parses a dotted version string into numeric segments. Returns false when
        /// the string is blank or any segment is not a non-negative integer.
        /// </summary>
        public static bool TryParse(string version, out ulong[] segments)
        {
            if (string.IsNullOrWhiteSpace(version))
            {
                segments = new ulong[0];
                return false;
            }

            var parts = version.Split('.');
            var result = new ulong[parts.Length];
            for (var i = 0; i < parts.Length; i++)
            {
                ulong value;
                if (!ulong.TryParse(parts[i].Trim(), out value))
                {
                    segments = new ulong[0];
                    return false;
                }
                result[i] = value;
            }
            segments = result;
            return true;
        }

        /// <summary>
        /// Compares two parsed version segment arrays. Missing segments are zero.
        /// Returns a negative value when a &lt; b, zero when equal, positive when a &gt; b.
        /// </summary>
        public static int CompareSegments(ulong[] a, ulong[] b)
        {
            var count = Math.Max(a.Length, b.Length);
            for (var i = 0; i < count; i++)
            {
                var left = i < a.Length ? a[i] : 0;
                var right = i < b.Length ? b[i] : 0;
                if (left != right)
                    return left > right ? 1 : -1;
            }
            return 0;
        }

        /// <summary>
        /// Compares two dotted version strings segment by segment. Blank or
        /// unparseable versions compare as the lowest possible version.
        /// </summary>
        public static int Compare(string a, string b)
        {
            ulong[] pa, pb;
            var aOk = TryParse(a, out pa);
            var bOk = TryParse(b, out pb);

            if (!aOk && !bOk) return 0;
            if (!aOk) return -1;
            if (!bOk) return 1;

            return CompareSegments(pa, pb);
        }
    }

    /// <summary>
    /// IComparer implementation of VersionHelpers.Compare for ordering version
    /// strings numerically (e.g. with LINQ OrderBy / GroupBy).
    /// </summary>
    public sealed class VersionComparer : IComparer<string>
    {
        public static readonly VersionComparer Instance = new VersionComparer();

        public int Compare(string x, string y)
        {
            return VersionHelpers.Compare(x, y);
        }
    }
}
