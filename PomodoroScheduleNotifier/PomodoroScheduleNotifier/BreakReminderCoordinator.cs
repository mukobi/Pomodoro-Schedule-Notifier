using System;

namespace PomodoroScheduleNotifier
{
    public interface IBreakReminderPresenter
    {
        bool IsReminderVisible { get; }

        void ShowReminder(DateTime nowLocal, PhaseState phaseState);

        void UpdateReminder(DateTime nowLocal, PhaseState phaseState);

        void CloseReminder();
    }

    public sealed class BreakReminderCoordinator
    {
        private readonly IBreakReminderPresenter presenter;
        private readonly IBreakReminderInterruptionDetector interruptionDetector;
        private readonly Func<DateTime> utcNow;

        private string? handledBreakKey;
        private string? pendingBreakKey;
        private DateTime? visibleSinceUtc;

        public BreakReminderCoordinator(
            IBreakReminderPresenter presenter,
            IBreakReminderInterruptionDetector interruptionDetector,
            Func<DateTime>? utcNow = null)
        {
            this.presenter = presenter;
            this.interruptionDetector = interruptionDetector;
            this.utcNow = utcNow ?? (() => DateTime.UtcNow);
        }

        public bool HasPendingReminder => pendingBreakKey != null;

        public void Reset()
        {
            pendingBreakKey = null;
            visibleSinceUtc = null;
            presenter.CloseReminder();
        }

        public void Update(DateTime nowLocal, PhaseState phaseState, UserSettings settings)
        {
            if (!settings.BreakReminderEnabled)
            {
                pendingBreakKey = null;
                visibleSinceUtc = null;
                presenter.CloseReminder();
                return;
            }

            if (!IsBreak(phaseState.Phase))
            {
                pendingBreakKey = null;
                visibleSinceUtc = null;
                presenter.CloseReminder();
                return;
            }

            string breakKey = CreateBreakKey(nowLocal, phaseState);

            if (presenter.IsReminderVisible)
            {
                presenter.UpdateReminder(nowLocal, phaseState);

                if (ShouldAutoClose(settings))
                {
                    visibleSinceUtc = null;
                    presenter.CloseReminder();
                }

                return;
            }

            visibleSinceUtc = null;

            if (handledBreakKey == breakKey)
            {
                return;
            }

            if (settings.BreakReminderSuppressDuringMeetingsAndSharing &&
                interruptionDetector.ShouldDeferBreakReminder(out _))
            {
                pendingBreakKey = breakKey;
                return;
            }

            pendingBreakKey = null;
            handledBreakKey = breakKey;
            visibleSinceUtc = utcNow();
            presenter.ShowReminder(nowLocal, phaseState);
        }

        private bool ShouldAutoClose(UserSettings settings)
        {
            if (!visibleSinceUtc.HasValue)
            {
                return false;
            }

            int maxVisibleMinutes = Math.Max(1, settings.BreakReminderMaxVisibleMinutes);
            return (utcNow() - visibleSinceUtc.Value).TotalMinutes >= maxVisibleMinutes;
        }

        private static bool IsBreak(CyclePhase phase)
        {
            return phase == CyclePhase.ShortBreak || phase == CyclePhase.LongBreak;
        }

        private static string CreateBreakKey(DateTime nowLocal, PhaseState phaseState)
        {
            int currentMinuteOfDay = (nowLocal.Hour * 60) + nowLocal.Minute;
            DateTime breakEnd = nowLocal.Date.AddMinutes(currentMinuteOfDay + phaseState.MinutesRemaining);
            return $"{phaseState.Phase}:{breakEnd:O}";
        }
    }
}
