using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bursa
{
    public static class Utilities
    {
        public static string BetterPlural(double amount, string unit)
        {
            amount = Math.Round(amount);

            if (amount == 1 || amount == -1)
                return string.Format("{0} {1}", amount, unit);

            if (unit.EndsWith("y") && !unit.EndsWith("ay"))
                unit = unit.Substring(0, unit.Length - 1) + "ie";

            return string.Format("{0} {1}s", amount, unit);
        }

        public static string TimeSpanToPrettyString(TimeSpan span, bool truncate_singular = false)
        {
            double total_years = span.TotalDays / 365d;
            double total_months = span.TotalDays / 30d;
            double total_weeks = span.TotalDays / 7d;

            int display_months = (int)(total_months % 12) + ((total_months >= 12) ? ((int)total_years == 0 ? 12 : 0) : 0);
            int display_weeks = (int)(total_weeks % 4) + ((total_weeks >= 4) ? (((int)total_months % 12) == 0 ? 4 : 0) : 0);

            Dictionary<string, int> lengths = new Dictionary<string, int>()
            {
                {BetterPlural(total_years, "year"), (int)(total_years) },
                {BetterPlural(display_months, "month"), display_months },
                {BetterPlural(display_weeks, "week"), display_weeks },
                {BetterPlural(span.TotalDays % 7, "day"), (int)(span.TotalDays % 7) },
                {BetterPlural(span.TotalHours % 24, "hour"), (int)(span.TotalHours % 24) },
                {BetterPlural(span.TotalMinutes % 60, "minute"), (int)(span.TotalMinutes % 60) },
                {BetterPlural(span.TotalSeconds % 60, "second"), (int)(span.TotalSeconds % 60) },
            };

            var valid_segments = lengths.Where(p => p.Value > 0);

            if (valid_segments.Count() >= 2)
                return string.Join(" ", valid_segments.Select(p => p.Key).Take(2));
            else if (valid_segments.Any())
            {
                var segment = valid_segments.Single();

                if (segment.Value == 1 && truncate_singular)
                    return segment.Key.Split(' ')[1];
                else
                    return segment.Key;
            }

            return span.ToString();
        }
    }
}
