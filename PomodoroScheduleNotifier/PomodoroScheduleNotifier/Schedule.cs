using System;

namespace PomodoroScheduleNotifier
{
    public enum CyclePhase
    {
        ShortBreak,
        LongBreak,
        Work,
        None
    }

    public readonly record struct PhaseState(CyclePhase Phase, int MinutesRemaining);

    public static class Schedule
    {
        public static PhaseState GetPhaseState(DateTime now)
        {
            return GetPhaseState((now.Hour * 60) + now.Minute);
        }

        public static PhaseState GetPhaseState(int minuteOfDay)
        {
            int minuteOfHalfHour = minuteOfDay % 30;

            // Long breaks start 5 minutes before hours divisible by 3 and last 35 minutes.
            bool inLongBreakWindow = ((minuteOfDay + 5) % 180) < 35;

            if (inLongBreakWindow)
            {
                int minutesRemaining = 35 - ((minuteOfDay + 5) % 180);
                return new PhaseState(CyclePhase.LongBreak, minutesRemaining);
            }

            if (minuteOfHalfHour >= 25)
            {
                int minutesRemaining = 30 - minuteOfHalfHour;
                return new PhaseState(CyclePhase.ShortBreak, minutesRemaining);
            }

            return new PhaseState(CyclePhase.Work, 25 - minuteOfHalfHour);
        }
    }
}
