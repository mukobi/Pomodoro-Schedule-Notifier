using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;

namespace PomodoroScheduleNotifier
{
    public partial class BreakReminderWindow : Window
    {
        private static readonly TimeSpan FadeDuration = TimeSpan.FromMilliseconds(350);
        private const double ProgressBarWidth = 500;
        private const double ProgressMarkerWidth = 4;
        private const double CompactHeadlineThreshold = 32;
        private const double MediumHeadlineThreshold = 48;
        private const double LongHeadlineThreshold = 64;
        private readonly BreakMessageRotator breakMessageRotator = new();
        private readonly StretchPromptRotator stretchPromptRotator = new();
        private bool isFadingOut;

        public BreakReminderWindow()
        {
            InitializeComponent();
            SourceInitialized += BreakReminderWindow_SourceInitialized;
        }

        public bool IsScheduledReminder { get; private set; }

        public void ShowForPhase(DateTime nowLocal, PhaseState phaseState, bool isScheduledReminder)
        {
            BeginAnimation(OpacityProperty, null);
            isFadingOut = false;
            IsScheduledReminder = isScheduledReminder;
            SetBreakMessage(breakMessageRotator.Next());
            StretchTargetText.Text = stretchPromptRotator.Next();
            UpdateForPhase(nowLocal, phaseState);

            Opacity = 0;
            CenterOnPrimaryScreen();
            Show();
            UpdateLayout();
            CenterOnPrimaryScreen();
            FadeTo(1);
            Activate();
            Focus();
        }

        public void UpdateForPhase(DateTime nowLocal, PhaseState phaseState)
        {
            PhaseText.Text = phaseState.Phase == CyclePhase.LongBreak ? "long break" : "short break";

            LongBreakProgressState progressState = LongBreakProgress.GetState(nowLocal, phaseState);
            PeriodProgressFillScale.ScaleX = progressState.PeriodProgress;
            NextLongBreakMarkerTransform.X = Math.Clamp(
                (progressState.NextLongBreakPosition * ProgressBarWidth) - (ProgressMarkerWidth / 2),
                0,
                ProgressBarWidth - ProgressMarkerWidth);
            PeriodStartText.Text = progressState.StartHourLabel;
            PeriodEndText.Text = progressState.EndHourLabel;
            NextLongBreakText.Text = progressState.NextLongBreakTimeLabel;
        }

        private void SetBreakMessage(string message)
        {
            double fontSize = GetBreakMessageFontSize(message);
            BreakMessageText.Text = message;
            BreakMessageText.FontSize = fontSize;
            BreakMessageText.LineHeight = fontSize * 1.08;
        }

        private static double GetBreakMessageFontSize(string message)
        {
            if (message.Length <= CompactHeadlineThreshold)
            {
                return 62;
            }

            if (message.Length <= MediumHeadlineThreshold)
            {
                return 52;
            }

            if (message.Length <= LongHeadlineThreshold)
            {
                return 44;
            }

            return 36;
        }

        public void HideReminder()
        {
            IsScheduledReminder = false;
            if (!IsVisible || isFadingOut)
            {
                return;
            }

            isFadingOut = true;
            DoubleAnimation animation = CreateFadeAnimation(0);
            animation.Completed += (_, _) =>
            {
                BeginAnimation(OpacityProperty, null);
                Opacity = 0;
                isFadingOut = false;
                Hide();
            };

            BeginAnimation(OpacityProperty, animation);
        }

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                HideReminder();
                e.Handled = true;
                return;
            }

            base.OnKeyDown(e);
        }

        private void DismissButton_Click(object sender, RoutedEventArgs e)
        {
            HideReminder();
        }

        private void BreakReminderWindow_SourceInitialized(object? sender, EventArgs e)
        {
            IntPtr handle = new WindowInteropHelper(this).Handle;
            _ = NativeMethods.SetWindowDisplayAffinity(handle, NativeMethods.WdaExcludeFromCapture);
        }

        private void FadeTo(double opacity)
        {
            BeginAnimation(OpacityProperty, CreateFadeAnimation(opacity));
        }

        private static DoubleAnimation CreateFadeAnimation(double opacity)
        {
            return new DoubleAnimation(opacity, FadeDuration)
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
        }

        private void CenterOnPrimaryScreen()
        {
            Screen primaryScreen = Screen.PrimaryScreen ?? Screen.FromHandle(new WindowInteropHelper(this).Handle);
            System.Drawing.Rectangle workingArea = primaryScreen.WorkingArea;

            using System.Drawing.Graphics graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
            double scaleX = 96.0 / graphics.DpiX;
            double scaleY = 96.0 / graphics.DpiY;

            double windowWidth = ActualWidth > 0 ? ActualWidth : Width;
            double windowHeight = ActualHeight > 0 ? ActualHeight : MinHeight;
            if (double.IsNaN(windowHeight) || windowHeight <= 0)
            {
                windowHeight = 460;
            }

            Left = (workingArea.Left * scaleX) + ((workingArea.Width * scaleX) - windowWidth) / 2;
            Top = (workingArea.Top * scaleY) + ((workingArea.Height * scaleY) - windowHeight) / 2;
        }
    }
}
