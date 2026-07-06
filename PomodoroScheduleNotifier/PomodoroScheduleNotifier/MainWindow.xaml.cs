using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Input;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace PomodoroScheduleNotifier
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IBreakReminderPresenter
    {
        string WorkSoundPath = @"C:\Windows\Media\Ring02.wav";
        string ShortBreakSoundPath = @"C:\Windows\Media\Alarm03.wav";
        string LongBreakSoundPath = @"C:\Windows\Media\Ring10.wav";

        NotifyIcon TrayIcon = new NotifyIcon();
        Icon? CurrentTrayIcon = null;
        double VolumeDb = 0;
        const double VolumeDbStep = 1.5;
        const int MaxTrayTextLength = 63;

        CyclePhase CurrentCyclePhase = CyclePhase.None;
        int TimeRemainingInPhase = 0;
        bool IsPaused = false;
        DateTime? LastDeactivatedUtc = null;
        UserSettings Settings = UserSettings.Load();
        BreakReminderCoordinator BreakReminderCoordinator;
        BreakReminderWindow? BreakReminderWindow;
        bool IsLoadingSettings = true;

        public MainWindow()
        {
            InitializeComponent();
            BreakMessageIconCache.Shared.Preload(BreakMessageRotator.StandardMessages);
            BreakReminderCoordinator = new BreakReminderCoordinator(
                this,
                new BreakReminderInterruptionDetector(),
                () => DateTime.UtcNow);
            Hide_Window();

            TrayIcon.Visible = true;
            TrayIcon.MouseClick += TrayIcon_MouseClick;

            VolumeDb = Settings.VolumeDb;
            UpdateVolumeDisplay();
            UpdateBreakReminderDisplay();
            IsLoadingSettings = false;
            UpdateStatusText();

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
            bool hadPendingReminder = BreakReminderCoordinator.HasPendingReminder;

            if (newCyclePhase != CurrentCyclePhase || newTimeRemainingInPhase != TimeRemainingInPhase)
            {
                if (newCyclePhase != CurrentCyclePhase)
                {
                    PlaySoundNotification(newCyclePhase);
                }

                CurrentCyclePhase = newCyclePhase;
                TimeRemainingInPhase = newTimeRemainingInPhase;
                UpdateTrayIcon();
                UpdateStatusText();
            }

            BreakReminderCoordinator.Update(DateTime.Now, phaseState, Settings);
            if (BreakReminderCoordinator.HasPendingReminder != hadPendingReminder)
            {
                UpdateTrayIcon();
                UpdateStatusText();
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
            toolTipName += " - " + TimeRemainingInPhase.ToString() + " min left";
            if (BreakReminderCoordinator.HasPendingReminder)
            {
                toolTipName += " - Reminder waiting";
            }

            SetTrayIcon(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", $"{iconFileName}.ico"));
            SetTrayText(toolTipName);
        }

        private void TrayIcon_MouseClick(object? sender, System.Windows.Forms.MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    TogglePaused();
                    break;
                case MouseButtons.Right:
                    if (IsVisible)
                    {
                        Hide_Window();
                    }
                    else
                    {
                        if (LastDeactivatedUtc.HasValue &&
                            (DateTime.UtcNow - LastDeactivatedUtc.Value).TotalMilliseconds < 250)
                        {
                            return;
                        }
                        Show_Window(GetMousePositionWindowsForms().X);
                    }
                    break;
                default:
                    break;
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            LastDeactivatedUtc = DateTime.UtcNow;
            Hide_Window();
        }

        private void Hide_Window()
        {
            Hide();
        }

        private void Show_Window(double cursorX)
        {
            double previousOpacity = Opacity;
            Opacity = 0;
            AdjustWindowPosition(cursorX);
            Show();
            UpdateLayout();
            AdjustWindowPosition(cursorX);
            Opacity = previousOpacity;
            Activate();
        }

        private void AdjustWindowPosition(double cursorX)
        {
            double windowWidth = ActualWidth > 0 ? ActualWidth : MinWidth;
            double windowHeight = ActualHeight > 0 ? ActualHeight : MinHeight;

            Screen sc = Screen.FromHandle(new WindowInteropHelper(this).Handle);
            if (sc.WorkingArea.Top > 0)
            {
                Rect desktopWorkingArea = SystemParameters.WorkArea;
                var middleOfWindow = desktopWorkingArea.Right - (windowWidth / 2);
                var gapToMiddle = middleOfWindow - cursorX;
                if (gapToMiddle < 0) gapToMiddle = 0;
                Left = desktopWorkingArea.Right - windowWidth - gapToMiddle;
                Top = desktopWorkingArea.Top;
            }

            else if ((sc.Bounds.Height - sc.WorkingArea.Height) > 0)
            {
                Rect desktopWorkingArea = SystemParameters.WorkArea;
                var middleOfWindow = desktopWorkingArea.Right - (windowWidth / 2);
                var gapToMiddle = middleOfWindow - cursorX;
                if (gapToMiddle < 0) gapToMiddle = 0;
                Left = desktopWorkingArea.Right - windowWidth - gapToMiddle;
                Top = desktopWorkingArea.Bottom - windowHeight;
            }
            else
            {
                Rect desktopWorkingArea = SystemParameters.WorkArea;
                Left = desktopWorkingArea.Right - windowWidth;
                Top = desktopWorkingArea.Bottom - windowHeight;
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
                SetTrayText("Paused, will not ring");
                BreakReminderCoordinator.Reset();
                UpdateStatusText();
            }
            else
            {
                Update();
                UpdateTrayIcon();
                UpdateStatusText();
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
            PlaySound(soundPath);
        }

        private void SetTrayIcon(string iconPath)
        {
            CurrentTrayIcon?.Dispose();
            CurrentTrayIcon = new Icon(iconPath);
            TrayIcon.Icon = CurrentTrayIcon;
        }

        private void PlaySound(string soundPath)
        {
            double linearGain = Math.Pow(10, VolumeDb / 20.0);
            var reader = new AudioFileReader(soundPath);
            var volumeProvider = new VolumeSampleProvider(reader)
            {
                Volume = (float)linearGain
            };

            var output = new WaveOutEvent();
            output.Init(volumeProvider);
            output.PlaybackStopped += (_, __) =>
            {
                output.Dispose();
                reader.Dispose();
            };
            output.Play();
        }

        private void UpdateVolumeDisplay()
        {
            if (VolumeDbText != null)
            {
                VolumeDbText.Text = VolumeDb.ToString("0.0");
            }
        }

        private void UpdateBreakReminderDisplay()
        {
            if (BreakReminderEnabledCheckBox != null)
            {
                BreakReminderEnabledCheckBox.IsChecked = Settings.BreakReminderEnabled;
            }

            if (SuppressDuringMeetingsCheckBox != null)
            {
                SuppressDuringMeetingsCheckBox.IsChecked = Settings.BreakReminderSuppressDuringMeetingsAndSharing;
            }
        }

        private void UpdateStatusText()
        {
            if (StatusText == null)
            {
                return;
            }

            if (IsPaused)
            {
                StatusText.Text = "Paused";
                return;
            }

            if (CurrentCyclePhase == CyclePhase.None)
            {
                StatusText.Text = "Initializing...";
                return;
            }

            string phaseLabel = CurrentCyclePhase switch
            {
                CyclePhase.LongBreak => "Long Break",
                CyclePhase.ShortBreak => "Short Break",
                CyclePhase.Work => "Work",
                _ => "Unknown"
            };

            StatusText.Text = $"{phaseLabel} - {TimeRemainingInPhase} min left";
            if (BreakReminderCoordinator.HasPendingReminder)
            {
                StatusText.Text += " - Reminder waiting";
            }
        }

        private void PlayPhaseButton_Click(object sender, RoutedEventArgs e)
        {
            CyclePhase phase = Schedule.GetPhaseState(DateTime.Now).Phase;
            PlaySoundNotification(phase);
        }

        private void VolumeDownButton_Click(object sender, RoutedEventArgs e)
        {
            VolumeDb -= VolumeDbStep;
            PersistVolume();
        }

        private void VolumeUpButton_Click(object sender, RoutedEventArgs e)
        {
            VolumeDb += VolumeDbStep;
            PersistVolume();
        }

        private void VolumeDbText_LostFocus(object sender, RoutedEventArgs e)
        {
            TryUpdateVolumeFromText();
        }

        private void VolumeDbText_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TryUpdateVolumeFromText();
                e.Handled = true;
            }
        }

        private void TryUpdateVolumeFromText()
        {
            if (VolumeDbText == null)
            {
                return;
            }

            if (double.TryParse(VolumeDbText.Text, out double newValue))
            {
                VolumeDb = newValue;
                PersistVolume();
            }
            else
            {
                UpdateVolumeDisplay();
            }
        }

        private void PersistVolume()
        {
            Settings.VolumeDb = VolumeDb;
            UserSettings.Save(Settings);
            UpdateVolumeDisplay();
        }

        private void BreakReminderEnabledCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (IsLoadingSettings || BreakReminderEnabledCheckBox == null)
            {
                return;
            }

            Settings.BreakReminderEnabled = BreakReminderEnabledCheckBox.IsChecked == true;
            UserSettings.Save(Settings);

            if (!Settings.BreakReminderEnabled)
            {
                BreakReminderCoordinator.Reset();
            }
        }

        private void SuppressDuringMeetingsCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (IsLoadingSettings || SuppressDuringMeetingsCheckBox == null)
            {
                return;
            }

            Settings.BreakReminderSuppressDuringMeetingsAndSharing = SuppressDuringMeetingsCheckBox.IsChecked == true;
            UserSettings.Save(Settings);
        }

        private void PreviewBreakReminderButton_Click(object sender, RoutedEventArgs e)
        {
            ShowBreakReminder(DateTime.Now, new PhaseState(CyclePhase.ShortBreak, 5), false);
        }

        private void RefreshQuoteImagesButton_Click(object sender, RoutedEventArgs e)
        {
            BreakMessageIconCache.Shared.Clear();
            BreakMessageIconCache.Shared.Preload(BreakMessageRotator.StandardMessages);
        }

        private void SetTrayText(string text)
        {
            TrayIcon.Text = text.Length <= MaxTrayTextLength
                ? text
                : text[..MaxTrayTextLength];
        }

        public bool IsReminderVisible => BreakReminderWindow?.IsVisible == true &&
                                         BreakReminderWindow.IsScheduledReminder;

        public void ShowReminder(DateTime nowLocal, PhaseState phaseState)
        {
            ShowBreakReminder(nowLocal, phaseState, true);
        }

        public void UpdateReminder(DateTime nowLocal, PhaseState phaseState)
        {
            BreakReminderWindow?.UpdateForPhase(nowLocal, phaseState);
        }

        public void CloseReminder()
        {
            if (BreakReminderWindow?.IsScheduledReminder == true)
            {
                BreakReminderWindow.HideReminder();
            }
        }

        private void ShowBreakReminder(DateTime nowLocal, PhaseState phaseState, bool isScheduledReminder)
        {
            if (BreakReminderWindow == null)
            {
                BreakReminderWindow = new BreakReminderWindow();
                BreakReminderWindow.Closed += (_, _) => BreakReminderWindow = null;
            }

            BreakReminderWindow.ShowForPhase(nowLocal, phaseState, isScheduledReminder);
        }
    }
}
