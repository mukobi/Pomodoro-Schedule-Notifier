using System;

namespace PomodoroScheduleNotifier
{
    public readonly record struct LongBreakProgressState(
        double PeriodProgress,
        double NextLongBreakPosition,
        string StartHourLabel,
        string EndHourLabel,
        string NextLongBreakTimeLabel);

    public static class LongBreakProgress
    {
        private const int LongBreakIntervalMinutes = 180;
        private const int LongBreakStartOffsetMinutes = 175;

        public static LongBreakProgressState GetState(DateTime nowLocal, PhaseState phaseState)
        {
            DateTime periodStart = GetPeriodStart(nowLocal);
            DateTime periodEnd = GetPeriodEnd(periodStart);
            DateTime longBreakStart = GetRelevantLongBreakStart(nowLocal, phaseState, periodStart);
            double periodMinutes = (periodEnd - periodStart).TotalMinutes;

            double periodProgress = Clamp01((nowLocal - periodStart).TotalMinutes / periodMinutes);
            double longBreakPosition = Clamp01((longBreakStart - periodStart).TotalMinutes / periodMinutes);

            return new LongBreakProgressState(
                periodProgress,
                longBreakPosition,
                GetHourLabel(periodStart),
                GetHourLabel(periodEnd),
                GetHourLabel(longBreakStart.AddMinutes(5)));
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

        private static DateTime GetRelevantLongBreakStart(DateTime nowLocal, PhaseState phaseState, DateTime periodStart)
        {
            if (phaseState.Phase == CyclePhase.LongBreak)
            {
                DateTime currentLongBreakStart = GetCurrentLongBreakStart(nowLocal);
                if (currentLongBreakStart >= periodStart)
                {
                    return currentLongBreakStart;
                }
            }

            return GetNextLongBreakStart(nowLocal);
        }

        private static string GetHourLabel(DateTime time)
        {
            return time.Hour.ToString();
        }

        private static DateTime GetCurrentLongBreakStart(DateTime nowLocal)
        {
            int minuteOfDay = (nowLocal.Hour * 60) + nowLocal.Minute;
            int elapsedMinutes = (minuteOfDay + 5) % LongBreakIntervalMinutes;
            return nowLocal.Date.AddMinutes(minuteOfDay - elapsedMinutes);
        }

        private static DateTime GetNextLongBreakStart(DateTime nowLocal)
        {
            DateTime start = nowLocal.Date.AddMinutes(LongBreakStartOffsetMinutes);
            while (start <= nowLocal)
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
