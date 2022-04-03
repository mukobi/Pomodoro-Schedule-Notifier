using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PomodoroScheduleNotifier
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// The number of minutes from midnight at which to start the schedule.
        /// </summary>
        int MinuteOffsetStart = 9 * 60;

        /// <summary>
        /// How many minutes a work period lasts
        /// </summary>
        int WorkDurationMinutes = 25;

        /// <summary>
        /// How many minutes a short break lasts.
        /// </summary>
        int ShortBreakDurationMinutes = 5;

        /// <summary>
        /// Every this many work periods, convert the work period into a long break.
        /// </summary>
        int LongBreakInterval = 6;


        String WorkSoundPath = @"C:\Windows\Media\Ring02.wav";
        String ShortBreakSoundPath = @"C:\Windows\Media\Alarm03.wav";
        String LongBreakSoundPath = @"C:\Windows\Media\Ring10.wav";

        NotifyIcon TrayIcon = new NotifyIcon();

        enum CyclePhase
        {
            ShortBreak,
            LongBreak,
            Work,
            None
        }
        CyclePhase CurrentCyclePhase = CyclePhase.None;
        int TimeRemainingInPhase = 0;

        public MainWindow()
        {
            InitializeComponent();
            Hide_Window();

            TrayIcon.Visible = true;
            TrayIcon.Click += TrayIcon_Click;

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
            int minutesSinceStart = (int)DateTime.Now.TimeOfDay.TotalMinutes - MinuteOffsetStart;

            int workCycleDuration = ShortBreakDurationMinutes + WorkDurationMinutes;
            int minutesSinceWorkCycleStart = minutesSinceStart % workCycleDuration;
            if (minutesSinceWorkCycleStart < 0)
            {
                minutesSinceWorkCycleStart += workCycleDuration;
            }

            int numWorkCyclesCompleted = minutesSinceStart / (workCycleDuration);

            CyclePhase newCyclePhase;
            int newTimeRemainingInPhase;

            if (numWorkCyclesCompleted % LongBreakInterval == 0)
            {
                newCyclePhase = CyclePhase.LongBreak;
                newTimeRemainingInPhase = workCycleDuration + ShortBreakDurationMinutes - minutesSinceWorkCycleStart;
            }
            else if (numWorkCyclesCompleted % LongBreakInterval == 1 && minutesSinceWorkCycleStart < ShortBreakDurationMinutes)
            {
                newCyclePhase = CyclePhase.LongBreak;
                newTimeRemainingInPhase = ShortBreakDurationMinutes - minutesSinceWorkCycleStart;
            }
            else if (minutesSinceWorkCycleStart < ShortBreakDurationMinutes)
            {
                newCyclePhase = CyclePhase.ShortBreak;
                newTimeRemainingInPhase = ShortBreakDurationMinutes - minutesSinceWorkCycleStart;
            }
            else
            {
                newCyclePhase = CyclePhase.Work;
                newTimeRemainingInPhase = workCycleDuration - minutesSinceWorkCycleStart;
            }

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

            TrayIcon.Icon = new Icon(@$"Resources/{iconFileName}.ico");
            TrayIcon.Text = toolTipName;
        }

        private void TrayIcon_Click(object? sender, EventArgs e)
        {
            System.Windows.Forms.MouseEventArgs mouseEvent = ((System.Windows.Forms.MouseEventArgs)e);
            switch (mouseEvent.Button)
            {
                case MouseButtons.Left:
                    // DEBUG play sound
                    PlaySoundNotification(CurrentCyclePhase);
                    Toggle_Silent();
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
            Graphics g = Graphics.FromHwnd(IntPtr.Zero);
            var pixelX = (int)((96 / g.DpiX) * point.X);
            var pixelY = (int)((96 / g.DpiY) * point.X);
            return new System.Windows.Point(pixelX, pixelY);
        }

        private void Toggle_Silent()
        {
            //throw new NotImplementedException();
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
            MediaPlayer player = new();
            player.Open(new Uri(soundPath));
            player.Play();
        }
    }
}