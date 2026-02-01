using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;

namespace PomodoroScheduleNotifier
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string WorkSoundPath = @"C:\Windows\Media\Ring02.wav";
        string ShortBreakSoundPath = @"C:\Windows\Media\Alarm03.wav";
        string LongBreakSoundPath = @"C:\Windows\Media\Ring10.wav";

        NotifyIcon TrayIcon = new NotifyIcon();
        Icon? CurrentTrayIcon = null;
        MediaPlayer? NotificationPlayer = null;

        CyclePhase CurrentCyclePhase = CyclePhase.None;
        int TimeRemainingInPhase = 0;
        bool IsPaused = false;

        public MainWindow()
        {
            InitializeComponent();
            Hide_Window();

            TrayIcon.Visible = true;
            TrayIcon.MouseClick += TrayIcon_MouseClick;

            // Set up update tick
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new();
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }

        private void DispatcherTimer_Tick(object? sender, EventArgs e)
        {
            Update();
        }

        /// <summary>
        /// Periodic update function. Checks the time and possibly updates UI/plays a sound.
        /// </summary>
        private void Update()
        {
            if (IsPaused)
            {
                return;
            }

            PhaseState phaseState = Schedule.GetPhaseState(DateTime.Now);
            CyclePhase newCyclePhase = phaseState.Phase;
            int newTimeRemainingInPhase = phaseState.MinutesRemaining;

            if (newCyclePhase != CurrentCyclePhase || newTimeRemainingInPhase != TimeRemainingInPhase)
            {
                if (newCyclePhase != CurrentCyclePhase)
                {
                    PlaySoundNotification(newCyclePhase);
                }

                CurrentCyclePhase = newCyclePhase;
                TimeRemainingInPhase = newTimeRemainingInPhase;
                UpdateTrayIcon();
            }
        }

        private void UpdateTrayIcon()
        {
            string iconFileName = CurrentCyclePhase switch
            {
                CyclePhase.LongBreak => "blue",
                CyclePhase.ShortBreak => "green",
                CyclePhase.Work => "red",
                _ => throw new NotImplementedException()
            };
            iconFileName += "-" + TimeRemainingInPhase.ToString();

            string toolTipName = CurrentCyclePhase switch
            {
                CyclePhase.LongBreak => "Long Break",
                CyclePhase.ShortBreak => "Short Break",
                CyclePhase.Work => "Work",
                _ => throw new NotImplementedException()
            };
            toolTipName += " - " + TimeRemainingInPhase.ToString() + " minutes remaining";

            SetTrayIcon(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", $"{iconFileName}.ico"));
            TrayIcon.Text = toolTipName;
        }

        private void TrayIcon_MouseClick(object? sender, System.Windows.Forms.MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    TogglePaused();
                    break;
                case MouseButtons.Right:
                    Show_Window(GetMousePositionWindowsForms().X);
                    break;
                default:
                    break;
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            Hide_Window();
        }

        private void Hide_Window()
        {
            Hide();
        }

        private void Show_Window(double cursorX)
        {
            AdjustWindowPosition(cursorX);
            Show();
            Activate();
        }

        private void AdjustWindowPosition(double cursorX)
        {
            Screen sc = Screen.FromHandle(new WindowInteropHelper(this).Handle);
            if (sc.WorkingArea.Top > 0)
            {
                Rect desktopWorkingArea = SystemParameters.WorkArea;
                var middleOfWindow = desktopWorkingArea.Right - (Width / 2);
                var gapToMiddle = middleOfWindow - cursorX;
                if (gapToMiddle < 0) gapToMiddle = 0;
                Left = desktopWorkingArea.Right - Width - gapToMiddle;
                Top = desktopWorkingArea.Top;
            }

            else if ((sc.Bounds.Height - sc.WorkingArea.Height) > 0)
            {
                Rect desktopWorkingArea = SystemParameters.WorkArea;
                var middleOfWindow = desktopWorkingArea.Right - (Width / 2);
                var gapToMiddle = middleOfWindow - cursorX;
                if (gapToMiddle < 0) gapToMiddle = 0;
                Left = desktopWorkingArea.Right - Width - gapToMiddle;
                Top = desktopWorkingArea.Bottom - Height;
            }
            else
            {
                Rect desktopWorkingArea = SystemParameters.WorkArea;
                Left = desktopWorkingArea.Right - Width;
                Top = desktopWorkingArea.Bottom - Height;
            }
        }

        public static System.Windows.Point GetMousePositionWindowsForms()
        {
            var point = System.Windows.Forms.Control.MousePosition;
            using Graphics g = Graphics.FromHwnd(IntPtr.Zero);
            var pixelX = (int)((96 / g.DpiX) * point.X);
            var pixelY = (int)((96 / g.DpiY) * point.Y);
            return new System.Windows.Point(pixelX, pixelY);
        }

        private void TogglePaused()
        {
            IsPaused = !IsPaused;
            if (IsPaused)
            {
                SetTrayIcon(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "paused.ico"));
                TrayIcon.Text = "Paused, will not ring";
            }
            else
            {
                Update();
                UpdateTrayIcon();
            }
        }

        private void PlaySoundNotification(CyclePhase cyclePhase)
        {
            string soundPath = cyclePhase switch
            {
                CyclePhase.LongBreak => LongBreakSoundPath,
                CyclePhase.ShortBreak => ShortBreakSoundPath,
                CyclePhase.Work => WorkSoundPath,
                _ => throw new NotImplementedException()
            };
            if (NotificationPlayer == null)
            {
                NotificationPlayer = new MediaPlayer();
            }

            NotificationPlayer.Stop();
            NotificationPlayer.Open(new Uri(soundPath));
            NotificationPlayer.Play();
        }

        private void SetTrayIcon(string iconPath)
        {
            CurrentTrayIcon?.Dispose();
            CurrentTrayIcon = new Icon(iconPath);
            TrayIcon.Icon = CurrentTrayIcon;
        }
    }
}
