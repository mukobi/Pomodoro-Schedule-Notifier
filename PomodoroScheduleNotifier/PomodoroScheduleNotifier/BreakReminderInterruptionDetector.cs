using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Win32;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using System.Text;

namespace PomodoroScheduleNotifier
{
    public interface IBreakReminderInterruptionDetector
    {
        bool ShouldDeferBreakReminder(out string reason);
    }

    internal readonly record struct CapabilityAccessRecord(
        string Capability,
        string Identity,
        bool IsActive);

    public sealed class BreakReminderInterruptionDetector : IBreakReminderInterruptionDetector
    {
        private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(3);

        private static readonly string[] CapabilityNames =
        {
            "microphone",
            "webcam",
            "graphicsCaptureProgrammatic",
            "graphicsCaptureWithoutBorder"
        };

        private static readonly string[] ScreenCaptureCapabilityNames =
        {
            "graphicscaptureprogrammatic",
            "graphicscapturewithoutborder"
        };

        private static readonly string[] MeetingProcessNames =
        {
            "teams",
            "ms-teams",
            "msteams",
            "microsoft teams",
            "zoom",
            "webex",
            "slack",
            "discord",
            "skype",
            "chime",
            "bluejeans",
            "gotomeeting"
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
            "in a call",
            "in a meeting",
            "screen sharing",
            "sharing screen",
            "sharing your screen",
            "you are sharing",
            "you're sharing",
            "you are presenting",
            "you're presenting",
            "presenting",
            "is presenting",
            "stop sharing",
            "stop presenting",
            "leave meeting",
            "raise hand",
            "call controls",
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

        private readonly Func<DateTime> utcNow;
        private readonly Func<IReadOnlyList<CapabilityAccessRecord>> capabilityAccessReader;
        private readonly Func<IReadOnlyList<AudioSessionRecord>> audioSessionReader;
        private DateTime? lastPollUtc;
        private bool cachedShouldDefer;
        private string cachedReason = string.Empty;

        public BreakReminderInterruptionDetector()
            : this(
                () => DateTime.UtcNow,
                ReadCapabilityAccessRecords,
                ReadAudioSessionRecords)
        {
        }

        internal BreakReminderInterruptionDetector(
            Func<DateTime> utcNow,
            Func<IReadOnlyList<CapabilityAccessRecord>> capabilityAccessReader,
            Func<IReadOnlyList<AudioSessionRecord>> audioSessionReader)
        {
            this.utcNow = utcNow;
            this.capabilityAccessReader = capabilityAccessReader;
            this.audioSessionReader = audioSessionReader;
        }

        public bool ShouldDeferBreakReminder(out string reason)
        {
            DateTime nowUtc = utcNow();
            if (lastPollUtc.HasValue &&
                nowUtc >= lastPollUtc.Value &&
                nowUtc - lastPollUtc.Value < PollInterval)
            {
                reason = cachedReason;
                return cachedShouldDefer;
            }

            cachedShouldDefer = ComputeShouldDefer(out cachedReason);
            lastPollUtc = nowUtc;

            reason = cachedReason;
            return cachedShouldDefer;
        }

        private bool ComputeShouldDefer(out string reason)
        {
            if (TryGetBlockingNotificationState(out reason))
            {
                return true;
            }

            if (TryFindActiveMeetingAudioSession(out reason))
            {
                return true;
            }

            if (TryFindActiveCapabilityAccess(out reason))
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

        private bool TryFindActiveMeetingAudioSession(out string reason)
        {
            reason = string.Empty;

            foreach (AudioSessionRecord session in audioSessionReader())
            {
                if (!ShouldDeferForAudioSession(session, out reason))
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        internal static bool ShouldDeferForAudioSession(AudioSessionRecord session, out string reason)
        {
            reason = string.Empty;

            if (!session.IsActive)
            {
                return false;
            }

            if (session.IsMicrophoneCapture)
            {
                reason = $"Active microphone audio session: {session.Identity}.";
                return true;
            }

            if (!LooksLikeDedicatedMeetingApp(session.Identity))
            {
                return false;
            }

            reason = $"Active meeting audio session: {session.Identity}.";
            return true;
        }

        private bool TryFindActiveCapabilityAccess(out string reason)
        {
            reason = string.Empty;

            foreach (CapabilityAccessRecord record in capabilityAccessReader())
            {
                if (ShouldDeferForCapability(record, out reason))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool ShouldDeferForCapability(CapabilityAccessRecord record, out string reason)
        {
            reason = string.Empty;

            if (!record.IsActive)
            {
                return false;
            }

            string capability = record.Capability.ToLowerInvariant();
            if (ContainsAny(capability, ScreenCaptureCapabilityNames))
            {
                reason = $"Active screen capture: {record.Identity}.";
                return true;
            }

            if (capability == "webcam")
            {
                reason = $"Active camera: {record.Identity}.";
                return true;
            }

            if (capability == "microphone")
            {
                reason = $"Active microphone: {record.Identity}.";
                return true;
            }

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

        private static IReadOnlyList<CapabilityAccessRecord> ReadCapabilityAccessRecords()
        {
            List<CapabilityAccessRecord> records = new();

            foreach (RegistryHive hive in new[] { RegistryHive.CurrentUser, RegistryHive.LocalMachine })
            {
                foreach (RegistryView view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
                {
                    try
                    {
                        using RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, view);
                        foreach (string capability in CapabilityNames)
                        {
                            using RegistryKey? capabilityKey = baseKey.OpenSubKey(
                                $@"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\{capability}");
                            if (capabilityKey == null)
                            {
                                continue;
                            }

                            ReadCapabilityKey(records, capability, capabilityKey, string.Empty);
                        }
                    }
                    catch
                    {
                    }
                }
            }

            return records;
        }

        private static void ReadCapabilityKey(
            List<CapabilityAccessRecord> records,
            string capability,
            RegistryKey key,
            string relativePath)
        {
            if (TryReadFileTime(key, "LastUsedTimeStart", out long start) &&
                start > 0)
            {
                _ = TryReadFileTime(key, "LastUsedTimeStop", out long stop);
                bool isActive = stop <= 0 || stop < start;
                string identity = BuildCapabilityIdentity(key, relativePath);
                records.Add(new CapabilityAccessRecord(capability, identity, isActive));
            }

            foreach (string subKeyName in key.GetSubKeyNames())
            {
                try
                {
                    using RegistryKey? subKey = key.OpenSubKey(subKeyName);
                    if (subKey == null)
                    {
                        continue;
                    }

                    string childPath = string.IsNullOrEmpty(relativePath)
                        ? subKeyName
                        : $"{relativePath}\\{subKeyName}";
                    ReadCapabilityKey(records, capability, subKey, childPath);
                }
                catch
                {
                }
            }
        }

        private static bool TryReadFileTime(RegistryKey key, string valueName, out long value)
        {
            value = 0;
            object? rawValue = key.GetValue(valueName);
            if (rawValue == null)
            {
                return false;
            }

            try
            {
                value = Convert.ToInt64(rawValue);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string BuildCapabilityIdentity(RegistryKey key, string relativePath)
        {
            List<string> parts = new();
            AddRegistryString(parts, key, "DisplayName");
            AddRegistryString(parts, key, "PackageFullName");

            if (!string.IsNullOrWhiteSpace(relativePath))
            {
                parts.Add(relativePath.Replace('#', '\\'));
            }

            return parts.Count == 0 ? "(unknown app)" : string.Join(" ", parts);
        }

        private static void AddRegistryString(List<string> parts, RegistryKey key, string valueName)
        {
            if (key.GetValue(valueName) is string value &&
                !string.IsNullOrWhiteSpace(value))
            {
                parts.Add(value);
            }
        }

        private static IReadOnlyList<AudioSessionRecord> ReadAudioSessionRecords()
        {
            List<AudioSessionRecord> records = new();

            try
            {
                using MMDeviceEnumerator enumerator = new();
                ReadDefaultAudioSessions(
                    enumerator,
                    DataFlow.Render,
                    Role.Multimedia,
                    isMicrophoneCapture: false,
                    records);
                ReadDefaultAudioSessions(
                    enumerator,
                    DataFlow.Capture,
                    Role.Communications,
                    isMicrophoneCapture: true,
                    records);
                ReadDefaultAudioSessions(
                    enumerator,
                    DataFlow.Capture,
                    Role.Multimedia,
                    isMicrophoneCapture: true,
                    records);
            }
            catch
            {
            }

            return records;
        }

        private static void ReadDefaultAudioSessions(
            MMDeviceEnumerator enumerator,
            DataFlow dataFlow,
            Role role,
            bool isMicrophoneCapture,
            List<AudioSessionRecord> records)
        {
            try
            {
                if (!enumerator.HasDefaultAudioEndpoint(dataFlow, role))
                {
                    return;
                }

                using MMDevice device = enumerator.GetDefaultAudioEndpoint(dataFlow, role);
                AudioSessionManager manager = device.AudioSessionManager;
                manager.RefreshSessions();
                SessionCollection sessions = manager.Sessions;

                for (int i = 0; i < sessions.Count; i++)
                {
                    using AudioSessionControl session = sessions[i];
                    if (session.IsSystemSoundsSession)
                    {
                        continue;
                    }

                    string processName = GetProcessName(session.GetProcessID);
                    string identity = string.Join(
                        " ",
                        processName,
                        session.DisplayName,
                        session.GetSessionIdentifier,
                        session.GetSessionInstanceIdentifier);
                    records.Add(new AudioSessionRecord(
                        identity,
                        session.State == AudioSessionState.AudioSessionStateActive,
                        isMicrophoneCapture));
                }
            }
            catch
            {
            }
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

        private static bool LooksLikeDedicatedMeetingApp(string value)
        {
            string normalizedValue = value.ToLowerInvariant();
            return ContainsAny(normalizedValue, MeetingProcessNames) ||
                   normalizedValue.Contains("microsoft teams", StringComparison.Ordinal) ||
                   normalizedValue.Contains("msteams", StringComparison.Ordinal);
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

        internal readonly record struct AudioSessionRecord(
            string Identity,
            bool IsActive,
            bool IsMicrophoneCapture = false);
    }
}
