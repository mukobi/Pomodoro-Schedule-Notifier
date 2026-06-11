using System;
using PomodoroScheduleNotifier;
using Xunit;

namespace PomodoroScheduleNotifier.Tests
{
    public class BreakReminderCoordinatorTests
    {
        [Fact]
        public void Update_ShowsOnceDuringBreak()
        {
            FakePresenter presenter = new();
            FakeDetector detector = new(false);
            DateTime utcNow = new(2026, 6, 11, 16, 25, 0, DateTimeKind.Utc);
            BreakReminderCoordinator coordinator = new(presenter, detector, () => utcNow);
            UserSettings settings = new();
            DateTime localNow = new(2026, 6, 11, 12, 25, 0);

            coordinator.Update(localNow, new PhaseState(CyclePhase.ShortBreak, 5), settings);
            presenter.HideAsUserWould();
            coordinator.Update(localNow.AddMinutes(1), new PhaseState(CyclePhase.ShortBreak, 4), settings);

            Assert.Equal(1, presenter.ShowCount);
            Assert.False(coordinator.HasPendingReminder);
        }

        [Fact]
        public void Update_DefersDuringMeetingAndShowsAfterItEnds()
        {
            FakePresenter presenter = new();
            FakeDetector detector = new(true);
            BreakReminderCoordinator coordinator = new(presenter, detector);
            UserSettings settings = new();
            DateTime localNow = new(2026, 6, 11, 12, 25, 0);

            coordinator.Update(localNow, new PhaseState(CyclePhase.ShortBreak, 5), settings);
            detector.ShouldDefer = false;
            coordinator.Update(localNow.AddMinutes(1), new PhaseState(CyclePhase.ShortBreak, 4), settings);
            coordinator.Update(localNow.AddMinutes(2), new PhaseState(CyclePhase.ShortBreak, 3), settings);

            Assert.Equal(1, presenter.ShowCount);
            Assert.True(presenter.IsReminderVisible);
            Assert.False(coordinator.HasPendingReminder);
        }

        [Fact]
        public void Update_DoesNotBuildQueueAcrossBreaks()
        {
            FakePresenter presenter = new();
            FakeDetector detector = new(true);
            BreakReminderCoordinator coordinator = new(presenter, detector);
            UserSettings settings = new();
            DateTime localNow = new(2026, 6, 11, 12, 25, 0);

            coordinator.Update(localNow, new PhaseState(CyclePhase.ShortBreak, 5), settings);
            coordinator.Update(localNow.AddMinutes(5), new PhaseState(CyclePhase.Work, 25), settings);
            detector.ShouldDefer = false;
            coordinator.Update(localNow.AddMinutes(30), new PhaseState(CyclePhase.ShortBreak, 5), settings);

            Assert.Equal(1, presenter.ShowCount);
            Assert.False(coordinator.HasPendingReminder);
        }

        [Fact]
        public void Update_ClosesVisibleReminderAfterConfiguredCap()
        {
            FakePresenter presenter = new();
            FakeDetector detector = new(false);
            DateTime utcNow = new(2026, 6, 11, 16, 25, 0, DateTimeKind.Utc);
            BreakReminderCoordinator coordinator = new(presenter, detector, () => utcNow);
            UserSettings settings = new();
            DateTime localNow = new(2026, 6, 11, 11, 55, 0);

            coordinator.Update(localNow, new PhaseState(CyclePhase.LongBreak, 35), settings);
            utcNow = utcNow.AddMinutes(5);
            coordinator.Update(localNow.AddMinutes(5), new PhaseState(CyclePhase.LongBreak, 30), settings);
            coordinator.Update(localNow.AddMinutes(6), new PhaseState(CyclePhase.LongBreak, 29), settings);

            Assert.False(presenter.IsReminderVisible);
            Assert.Equal(1, presenter.ShowCount);
            Assert.Equal(1, presenter.CloseCount);
        }

        [Fact]
        public void Update_ClosesAndSuppressesWhenDisabled()
        {
            FakePresenter presenter = new();
            FakeDetector detector = new(false);
            BreakReminderCoordinator coordinator = new(presenter, detector);
            UserSettings settings = new();
            DateTime localNow = new(2026, 6, 11, 12, 25, 0);

            coordinator.Update(localNow, new PhaseState(CyclePhase.ShortBreak, 5), settings);
            settings.BreakReminderEnabled = false;
            coordinator.Update(localNow.AddMinutes(1), new PhaseState(CyclePhase.ShortBreak, 4), settings);

            Assert.False(presenter.IsReminderVisible);
            Assert.Equal(1, presenter.ShowCount);
            Assert.Equal(1, presenter.CloseCount);
        }

        private sealed class FakePresenter : IBreakReminderPresenter
        {
            public bool IsReminderVisible { get; private set; }

            public int ShowCount { get; private set; }

            public int CloseCount { get; private set; }

            public void ShowReminder(DateTime nowLocal, PhaseState phaseState)
            {
                ShowCount++;
                IsReminderVisible = true;
            }

            public void UpdateReminder(DateTime nowLocal, PhaseState phaseState)
            {
            }

            public void CloseReminder()
            {
                if (IsReminderVisible)
                {
                    CloseCount++;
                    IsReminderVisible = false;
                }
            }

            public void HideAsUserWould()
            {
                IsReminderVisible = false;
            }
        }

        private sealed class FakeDetector : IBreakReminderInterruptionDetector
        {
            public FakeDetector(bool shouldDefer)
            {
                ShouldDefer = shouldDefer;
            }

            public bool ShouldDefer { get; set; }

            public bool ShouldDeferBreakReminder(out string reason)
            {
                reason = ShouldDefer ? "busy" : string.Empty;
                return ShouldDefer;
            }
        }
    }
}
