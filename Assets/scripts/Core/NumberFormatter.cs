using System;

namespace ShellingOut
{
    /// Formats large numbers: 1234 -> "1.23K", 5.6e9 -> "5.6B",
    /// then aa, ab, ac... beyond T. Also formats durations for offline popups.
    public static class NumberFormatter
    {
        static readonly string[] BaseSuffixes = { "", "K", "M", "B", "T" };

        public static string Format(double value)
        {
            if (double.IsNaN(value)) return "0";
            if (double.IsInfinity(value)) return "∞";

            string sign = value < 0 ? "-" : "";
            value = Math.Abs(value);

            if (value < 1000d)
                return sign + (value < 10d ? value.ToString("0.#") : value.ToString("0"));

            int tier = (int)Math.Floor(Math.Log10(value) / 3d);
            double mantissa = value / Math.Pow(10d, tier * 3);

            // Rounding can push 999.999 to 1000 -- bump the tier instead of printing "1000K".
            if (Math.Round(mantissa, 2) >= 1000d)
            {
                tier++;
                mantissa = 1d;
            }

            return sign + mantissa.ToString("0.##") + Suffix(tier);
        }

        static string Suffix(int tier)
        {
            if (tier < BaseSuffixes.Length) return BaseSuffixes[tier];
            int n = tier - BaseSuffixes.Length; // 0 -> aa, 1 -> ab ...
            char first = (char)('a' + n / 26);
            char second = (char)('a' + n % 26);
            return $"{first}{second}";
        }

        public static string FormatRate(double perSecond) => Format(perSecond) + "/s";

        public static string FormatDuration(double seconds)
        {
            var t = TimeSpan.FromSeconds(Math.Max(0, seconds));
            if (t.TotalDays >= 1d) return $"{(int)t.TotalDays}d {t.Hours}h";
            if (t.TotalHours >= 1d) return $"{(int)t.TotalHours}h {t.Minutes}m";
            if (t.TotalMinutes >= 1d) return $"{(int)t.TotalMinutes}m {t.Seconds}s";
            return $"{(int)t.TotalSeconds}s";
        }
    }
}
