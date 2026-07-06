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
        private PendingReminder? pendingReminder;
        private string? pendingReminderReason;
        private DateTime? visibleSinceUtc;
        private PhaseState? deferredVisiblePhaseState;

        public BreakReminderCoordinator(
            IBreakReminderPresenter presenter,
            IBreakReminderInterruptionDetector interruptionDetector,
            Func<DateTime>? utcNow = null)
        {
            this.presenter = presenter;
            this.interruptionDetector = interruptionDetector;
            this.utcNow = utcNow ?? (() => DateTime.UtcNow);
        }

        public bool HasPendingReminder => pendingReminder.HasValue;

        public string? PendingReminderReason => pendingReminderReason;

        public void Reset()
        {
            pendingReminder = null;
            pendingReminderReason = null;
            visibleSinceUtc = null;
            deferredVisiblePhaseState = null;
            presenter.CloseReminder();
        }

        public void Update(DateTime nowLocal, PhaseState phaseState, UserSettings settings)
        {
            if (!settings.BreakReminderEnabled)
            {
                pendingReminder = null;
                pendingReminderReason = null;
                visibleSinceUtc = null;
                deferredVisiblePhaseState = null;
                presenter.CloseReminder();
                return;
            }

            if (!IsBreak(phaseState.Phase))
            {
                HandleNonBreak(nowLocal, phaseState, settings);
                return;
            }

            string breakKey = CreateBreakKey(nowLocal, phaseState);

            if (presenter.IsReminderVisible)
            {
                deferredVisiblePhaseState = null;
                presenter.UpdateReminder(nowLocal, phaseState);

                if (ShouldAutoClose(settings))
                {
                    visibleSinceUtc = null;
                    deferredVisiblePhaseState = null;
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
                interruptionDetector.ShouldDeferBreakReminder(out string reason))
            {
                SetPendingReminder(breakKey, phaseState, reason);
                return;
            }

            pendingReminder = null;
            pendingReminderReason = null;
            handledBreakKey = breakKey;
            visibleSinceUtc = utcNow();
            deferredVisiblePhaseState = null;
            presenter.ShowReminder(nowLocal, phaseState);
        }

        private void HandleNonBreak(DateTime nowLocal, PhaseState phaseState, UserSettings settings)
        {
            if (presenter.IsReminderVisible)
            {
                if (deferredVisiblePhaseState.HasValue)
                {
                    presenter.UpdateReminder(nowLocal, deferredVisiblePhaseState.Value);

                    if (ShouldAutoClose(settings))
                    {
                        visibleSinceUtc = null;
                        deferredVisiblePhaseState = null;
                        presenter.CloseReminder();
                    }

                    return;
                }

                visibleSinceUtc = null;
                presenter.CloseReminder();
                return;
            }

            visibleSinceUtc = null;
            deferredVisiblePhaseState = null;

            if (!pendingReminder.HasValue)
            {
                pendingReminderReason = null;
                return;
            }

            if (settings.BreakReminderSuppressDuringMeetingsAndSharing &&
                interruptionDetector.ShouldDeferBreakReminder(out string reason))
            {
                pendingReminderReason = reason;
                return;
            }

            PendingReminder reminder = pendingReminder.Value;
            pendingReminder = null;
            pendingReminderReason = null;
            handledBreakKey = reminder.BreakKey;
            visibleSinceUtc = utcNow();
            deferredVisiblePhaseState = reminder.PhaseState;
            presenter.ShowReminder(nowLocal, reminder.PhaseState);
        }

        private void SetPendingReminder(string breakKey, PhaseState phaseState, string reason)
        {
            if (pendingReminder.HasValue &&
                pendingReminder.Value.PhaseState.Phase == CyclePhase.LongBreak &&
                phaseState.Phase != CyclePhase.LongBreak)
            {
                pendingReminderReason = reason;
                return;
            }

            pendingReminder = new PendingReminder(breakKey, phaseState);
            pendingReminderReason = reason;
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

        private readonly record struct PendingReminder(string BreakKey, PhaseState PhaseState);
    }
}
