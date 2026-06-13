using System;
using System.Collections.Generic;

namespace PomodoroScheduleNotifier
{
    public readonly record struct LongBreakProgressMarker(double Position, string HourLabel);

    public readonly record struct LongBreakProgressState(
        double PeriodProgress,
        IReadOnlyList<LongBreakProgressMarker> LongBreakMarkers,
        string StartHourLabel,
        string EndHourLabel);

    public static class LongBreakProgress
    {
        private const int LongBreakIntervalMinutes = 180;
        private const int LongBreakStartOffsetMinutes = 175;

        public static LongBreakProgressState GetState(DateTime nowLocal, PhaseState phaseState)
        {
            DateTime periodStart = GetPeriodStart(nowLocal);
            DateTime periodEnd = GetPeriodEnd(periodStart);
            double periodMinutes = (periodEnd - periodStart).TotalMinutes;

            double periodProgress = Clamp01((nowLocal - periodStart).TotalMinutes / periodMinutes);

            return new LongBreakProgressState(
                periodProgress,
                GetLongBreakMarkers(periodStart, periodEnd),
                GetHourLabel(periodStart),
                GetHourLabel(periodEnd));
        }

        private static DateTime GetPeriodStart(DateTime nowLocal)
        {
            DateTime midnight = nowLocal.Date;
            DateTime morning = nowLocal.Date.AddHours(9);
            DateTime evening = nowLocal.Date.AddHours(18);

            if (nowLocal < morning)
            {
                return midnight;
            }

            if (nowLocal < evening)
            {
                return morning;
            }

            return evening;
        }

        private static DateTime GetPeriodEnd(DateTime periodStart)
        {
            return periodStart.Hour switch
            {
                0 => periodStart.Date.AddHours(9),
                9 => periodStart.Date.AddHours(18),
                18 => periodStart.Date.AddDays(1),
                _ => throw new ArgumentOutOfRangeException(nameof(periodStart), "Unexpected period start.")
            };
        }

        private static string GetHourLabel(DateTime time)
        {
            return time.Hour.ToString();
        }

        private static IReadOnlyList<LongBreakProgressMarker> GetLongBreakMarkers(DateTime periodStart, DateTime periodEnd)
        {
            double periodMinutes = (periodEnd - periodStart).TotalMinutes;
            DateTime longBreakStart = GetFirstLongBreakStartOnOrAfter(periodStart);
            List<LongBreakProgressMarker> markers = new();

            while (longBreakStart < periodEnd)
            {
                markers.Add(new LongBreakProgressMarker(
                    Clamp01((longBreakStart - periodStart).TotalMinutes / periodMinutes),
                    GetHourLabel(longBreakStart.AddMinutes(5))));
                longBreakStart = longBreakStart.AddMinutes(LongBreakIntervalMinutes);
            }

            return markers;
        }

        private static DateTime GetFirstLongBreakStartOnOrAfter(DateTime time)
        {
            DateTime start = time.Date.AddMinutes(LongBreakStartOffsetMinutes);
            while (start < time)
            {
                start = start.AddMinutes(LongBreakIntervalMinutes);
            }

            return start;
        }

        private static double Clamp01(double value)
        {
            return Math.Clamp(value, 0, 1);
        }
    }
}
