using System;
using System.Diagnostics;
using System.Text;

namespace PomodoroScheduleNotifier
{
    public interface IBreakReminderInterruptionDetector
    {
        bool ShouldDeferBreakReminder(out string reason);
    }

    public sealed class BreakReminderInterruptionDetector : IBreakReminderInterruptionDetector
    {
        private static readonly string[] MeetingProcessNames =
        {
            "teams",
            "ms-teams",
            "msteams",
            "zoom",
            "webex",
            "slack",
            "discord",
            "skype"
        };

        private static readonly string[] BrowserProcessNames =
        {
            "chrome",
            "msedge",
            "firefox",
            "brave",
            "opera"
        };

        private static readonly string[] MeetingTitleSignals =
        {
            "meeting",
            "call",
            "screen sharing",
            "sharing screen",
            "you are sharing",
            "you're sharing",
            "presenting",
            "is presenting",
            "stop sharing",
            "google meet",
            "teams meeting",
            "teams call",
            "zoom meeting",
            "webex meeting",
            "slack huddle",
            "huddle"
        };

        private static readonly string[] BrowserMeetingSignals =
        {
            "google meet",
            "meet.google.com",
            "teams.microsoft.com",
            "zoom meeting",
            "webex meeting"
        };

        public bool ShouldDeferBreakReminder(out string reason)
        {
            if (TryGetBlockingNotificationState(out reason))
            {
                return true;
            }

            if (TryFindMeetingOrSharingWindow(out reason))
            {
                return true;
            }

            reason = string.Empty;
            return false;
        }

        private static bool TryGetBlockingNotificationState(out string reason)
        {
            reason = string.Empty;

            try
            {
                int result = NativeMethods.SHQueryUserNotificationState(out QueryUserNotificationState state);
                if (result != 0 || state == QueryUserNotificationState.AcceptsNotifications)
                {
                    return false;
                }

                reason = $"Windows notification state is {state}.";
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryFindMeetingOrSharingWindow(out string reason)
        {
            string foundReason = string.Empty;

            try
            {
                NativeMethods.EnumWindows((windowHandle, _) =>
                {
                    if (!NativeMethods.IsWindowVisible(windowHandle))
                    {
                        return true;
                    }

                    string title = GetWindowTitle(windowHandle);
                    if (string.IsNullOrWhiteSpace(title))
                    {
                        return true;
                    }

                    NativeMethods.GetWindowThreadProcessId(windowHandle, out uint processId);
                    string processName = GetProcessName(processId);

                    if (LooksLikeMeetingOrSharingWindow(processName, title))
                    {
                        foundReason = $"{processName}: {title}";
                        return false;
                    }

                    return true;
                }, IntPtr.Zero);
            }
            catch
            {
                reason = string.Empty;
                return false;
            }

            reason = foundReason;
            return !string.IsNullOrEmpty(foundReason);
        }

        private static string GetWindowTitle(IntPtr windowHandle)
        {
            int length = NativeMethods.GetWindowTextLength(windowHandle);
            if (length <= 0)
            {
                return string.Empty;
            }

            StringBuilder title = new(length + 1);
            _ = NativeMethods.GetWindowText(windowHandle, title, title.Capacity);
            return title.ToString();
        }

        private static string GetProcessName(uint processId)
        {
            try
            {
                using Process process = Process.GetProcessById((int)processId);
                return process.ProcessName;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static bool LooksLikeMeetingOrSharingWindow(string processName, string title)
        {
            string normalizedProcessName = processName.ToLowerInvariant();
            string normalizedTitle = title.ToLowerInvariant();

            bool titleHasMeetingSignal = ContainsAny(normalizedTitle, MeetingTitleSignals);
            bool processIsMeetingApp = ContainsAny(normalizedProcessName, MeetingProcessNames);
            bool processIsBrowser = ContainsAny(normalizedProcessName, BrowserProcessNames);
            bool browserHasMeetingSignal = ContainsAny(normalizedTitle, BrowserMeetingSignals);
            bool titleHasStrongSharingSignal =
                normalizedTitle.Contains("screen sharing", StringComparison.Ordinal) ||
                normalizedTitle.Contains("sharing screen", StringComparison.Ordinal) ||
                normalizedTitle.Contains("you are sharing", StringComparison.Ordinal) ||
                normalizedTitle.Contains("you're sharing", StringComparison.Ordinal) ||
                normalizedTitle.Contains("stop sharing", StringComparison.Ordinal) ||
                normalizedTitle.Contains("presenting", StringComparison.Ordinal);

            return titleHasStrongSharingSignal ||
                   (processIsMeetingApp && titleHasMeetingSignal) ||
                   (processIsBrowser && browserHasMeetingSignal);
        }

        private static bool ContainsAny(string value, string[] needles)
        {
            foreach (string needle in needles)
            {
                if (value.Contains(needle, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
