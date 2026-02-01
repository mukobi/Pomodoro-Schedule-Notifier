using System;
using PomodoroScheduleNotifier;
using Xunit;

namespace PomodoroScheduleNotifier.Tests
{
    public class ScheduleTests
    {
        [Theory]
        [InlineData(11, 54, CyclePhase.Work, 1)]
        [InlineData(11, 55, CyclePhase.LongBreak, 35)]
        [InlineData(12, 0, CyclePhase.LongBreak, 30)]
        [InlineData(12, 29, CyclePhase.LongBreak, 1)]
        [InlineData(12, 30, CyclePhase.Work, 25)]
        [InlineData(12, 25, CyclePhase.LongBreak, 5)]
        [InlineData(13, 25, CyclePhase.ShortBreak, 5)]
        [InlineData(13, 29, CyclePhase.ShortBreak, 1)]
        [InlineData(13, 30, CyclePhase.Work, 25)]
        [InlineData(14, 55, CyclePhase.LongBreak, 35)]
        [InlineData(0, 0, CyclePhase.LongBreak, 30)]
        public void GetPhaseState_UsesExpectedSchedule(int hour, int minute, CyclePhase expectedPhase, int expectedRemaining)
        {
            int minuteOfDay = (hour * 60) + minute;
            PhaseState result = Schedule.GetPhaseState(minuteOfDay);

            Assert.Equal(expectedPhase, result.Phase);
            Assert.Equal(expectedRemaining, result.MinutesRemaining);
        }
    }
}
