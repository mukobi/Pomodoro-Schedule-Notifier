using System;
using PomodoroScheduleNotifier;
using Xunit;

namespace PomodoroScheduleNotifier.Tests
{
    public class LongBreakProgressTests
    {
        [Theory]
        [InlineData(2026, 6, 11, 10, 25, "9", "18", 85.0 / 540.0)]
        [InlineData(2026, 6, 11, 19, 25, "18", "0", 85.0 / 360.0)]
        [InlineData(2026, 6, 11, 1, 25, "0", "9", 85.0 / 540.0)]
        public void GetState_UsesExpectedTimeBlock(
            int year,
            int month,
            int day,
            int hour,
            int minute,
            string expectedStartHourLabel,
            string expectedEndHourLabel,
            double expectedPeriodProgress)
        {
            DateTime now = new(year, month, day, hour, minute, 0);
            LongBreakProgressState result = LongBreakProgress.GetState(
                now,
                new PhaseState(CyclePhase.ShortBreak, 5));

            Assert.Equal(expectedStartHourLabel, result.StartHourLabel);
            Assert.Equal(expectedEndHourLabel, result.EndHourLabel);
            Assert.Equal(expectedPeriodProgress, result.PeriodProgress, 4);
        }

        [Fact]
        public void GetState_ReturnsAllLongBreakMarkersInDaytimePeriod()
        {
            DateTime now = new(2026, 6, 11, 10, 25, 0);
            LongBreakProgressState result = LongBreakProgress.GetState(
                now,
                new PhaseState(CyclePhase.ShortBreak, 5));

            AssertMarkers(
                result,
                ("12", 175.0 / 540.0),
                ("15", 355.0 / 540.0),
                ("18", 535.0 / 540.0));
        }

        [Fact]
        public void GetState_ReturnsAllLongBreakMarkersInEveningPeriod()
        {
            DateTime now = new(2026, 6, 11, 19, 25, 0);
            LongBreakProgressState result = LongBreakProgress.GetState(
                now,
                new PhaseState(CyclePhase.ShortBreak, 5));

            AssertMarkers(
                result,
                ("21", 175.0 / 360.0),
                ("0", 355.0 / 360.0));
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
            Assert.Equal(505.0 / 540.0, result.PeriodProgress, 4);
            AssertMarkers(
                result,
                ("3", 175.0 / 540.0),
                ("6", 355.0 / 540.0),
                ("9", 535.0 / 540.0));
        }

        [Fact]
        public void GetState_DuringLongBreakCrossingEveningPeriodStart_UsesEveningPeriodMarkers()
        {
            DateTime now = new(2026, 6, 11, 18, 10, 0);
            LongBreakProgressState result = LongBreakProgress.GetState(
                now,
                new PhaseState(CyclePhase.LongBreak, 20));

            Assert.Equal("18", result.StartHourLabel);
            Assert.Equal("0", result.EndHourLabel);
            AssertMarkers(
                result,
                ("21", 175.0 / 360.0),
                ("0", 355.0 / 360.0));
        }

        private static void AssertMarkers(
            LongBreakProgressState result,
            params (string Label, double Position)[] expectedMarkers)
        {
            Assert.Equal(expectedMarkers.Length, result.LongBreakMarkers.Count);
            for (int i = 0; i < expectedMarkers.Length; i++)
            {
                Assert.Equal(expectedMarkers[i].Label, result.LongBreakMarkers[i].HourLabel);
                Assert.Equal(expectedMarkers[i].Position, result.LongBreakMarkers[i].Position, 4);
            }
        }
    }
}
