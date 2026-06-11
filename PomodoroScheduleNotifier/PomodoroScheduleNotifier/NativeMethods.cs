using System;
using System.Runtime.InteropServices;
using System.Text;

namespace PomodoroScheduleNotifier
{
    internal enum QueryUserNotificationState
    {
        NotPresent = 1,
        Busy = 2,
        RunningD3DFullScreen = 3,
        PresentationMode = 4,
        AcceptsNotifications = 5,
        QuietTime = 6,
        App = 7
    }

    internal static class NativeMethods
    {
        internal const uint WdaExcludeFromCapture = 0x00000011;

        internal delegate bool EnumWindowsProc(IntPtr windowHandle, IntPtr lParam);

        [DllImport("shell32.dll")]
        internal static extern int SHQueryUserNotificationState(out QueryUserNotificationState state);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool EnumWindows(EnumWindowsProc enumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWindowVisible(IntPtr windowHandle);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern int GetWindowTextLength(IntPtr windowHandle);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern int GetWindowText(IntPtr windowHandle, StringBuilder text, int maxCount);

        [DllImport("user32.dll")]
        internal static extern uint GetWindowThreadProcessId(IntPtr windowHandle, out uint processId);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetWindowDisplayAffinity(IntPtr windowHandle, uint affinity);
    }
}
