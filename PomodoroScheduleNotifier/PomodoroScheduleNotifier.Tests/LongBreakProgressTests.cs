using System;
using PomodoroScheduleNotifier;
using Xunit;

namespace PomodoroScheduleNotifier.Tests
{
    public class LongBreakProgressTests
    {
        [Theory]
        [InlineData(2026, 6, 11, 10, 25, "9", "18", "12", 85.0 / 540.0, 175.0 / 540.0)]
        [InlineData(2026, 6, 11, 19, 25, "18", "0", "21", 85.0 / 360.0, 175.0 / 360.0)]
        [InlineData(2026, 6, 11, 1, 25, "0", "9", "3", 85.0 / 540.0, 175.0 / 540.0)]
        public void GetState_UsesExpectedTimeBlock(
            int year,
            int month,
            int day,
            int hour,
            int minute,
            string expectedStartHourLabel,
            string expectedEndHourLabel,
            string expectedNextLongBreakTimeLabel,
            double expectedPeriodProgress,
            double expectedLongBreakPosition)
        {
            DateTime now = new(year, month, day, hour, minute, 0);
            LongBreakProgressState result = LongBreakProgress.GetState(
                now,
                new PhaseState(CyclePhase.ShortBreak, 5));

            Assert.Equal(expectedStartHourLabel, result.StartHourLabel);
            Assert.Equal(expectedEndHourLabel, result.EndHourLabel);
            Assert.Equal(expectedNextLongBreakTimeLabel, result.NextLongBreakTimeLabel);
            Assert.Equal(expectedPeriodProgress, result.PeriodProgress, 4);
            Assert.Equal(expectedLongBreakPosition, result.NextLongBreakPosition, 4);
        }

        [Fact]
        public void GetState_UsesCurrentLongBreakInsidePeriod()
        {
            DateTime now = new(2026, 6, 11, 11, 56, 0);
            LongBreakProgressState result = LongBreakProgress.GetState(
                now,
                new PhaseState(CyclePhase.LongBreak, 34));

            Assert.Equal("12", result.NextLongBreakTimeLabel);
            Assert.Equal(175.0 / 540.0, result.NextLongBreakPosition, 4);
        }

        [Fact]
        public void GetState_UsesMidnightPeriodBeforeNineAm()
        {
            DateTime now = new(2026, 6, 11, 8, 25, 0);
            LongBreakProgressState result = LongBreakProgress.GetState(
                now,
                new PhaseState(CyclePhase.ShortBreak, 5));

            Assert.Equal("0", result.StartHourLabel);
            Assert.Equal("9", result.EndHourLabel);
            Assert.Equal("9", result.NextLongBreakTimeLabel);
            Assert.Equal(505.0 / 540.0, result.PeriodProgress, 4);
            Assert.Equal(535.0 / 540.0, result.NextLongBreakPosition, 4);
        }

        [Fact]
        public void GetState_DuringLongBreakCrossingEveningPeriodStart_UsesNextLongBreakInEveningPeriod()
        {
            DateTime now = new(2026, 6, 11, 18, 10, 0);
            LongBreakProgressState result = LongBreakProgress.GetState(
                now,
                new PhaseState(CyclePhase.LongBreak, 20));

            Assert.Equal("18", result.StartHourLabel);
            Assert.Equal("0", result.EndHourLabel);
            Assert.Equal("21", result.NextLongBreakTimeLabel);
            Assert.Equal(175.0 / 360.0, result.NextLongBreakPosition, 4);
        }
    }
}
