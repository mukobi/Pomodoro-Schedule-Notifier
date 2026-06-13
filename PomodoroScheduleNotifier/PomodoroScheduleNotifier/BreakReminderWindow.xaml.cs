using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace PomodoroScheduleNotifier
{
    public partial class BreakReminderWindow : Window
    {
        private static readonly TimeSpan FadeDuration = TimeSpan.FromMilliseconds(350);
        private const double ProgressBarWidth = 500;
        private const double ProgressMarkerWidth = 4;
        private const double ProgressMarkerHeight = 30;
        private const double ProgressLabelWidth = 44;
        private const double CompactHeadlineThreshold = 32;
        private const double MediumHeadlineThreshold = 48;
        private const double LongHeadlineThreshold = 64;
        private static readonly Color ProgressEarlyColor = Color.FromRgb(0x4E, 0x83, 0x78);
        private static readonly Color ProgressMiddleColor = Color.FromRgb(0xD8, 0xC2, 0x4B);
        private static readonly Color ProgressLateColor = Color.FromRgb(0xB8, 0x62, 0x48);
        private static readonly Brush ProgressMarkerBrush = CreateBrush("#F4F1E8");
        private static readonly Brush ProgressPassedMarkerBrush = CreateBrush("#5D625C");
        private static readonly Brush ProgressTickLabelBrush = CreateBrush("#B9BEB5");
        private static readonly Brush ProgressPassedTickLabelBrush = CreateBrush("#656A63");
        private static readonly Brush ProgressEndpointLabelBrush = CreateBrush("#858B83");
        private readonly BreakMessageRotator breakMessageRotator = new();
        private readonly BreakMessageIconCache breakMessageIconCache = BreakMessageIconCache.Shared;
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
            PeriodProgressFill.Fill = CreateProgressFillBrush(progressState.PeriodProgress);
            RenderProgressMarkers(progressState);
        }

        private void SetBreakMessage(BreakMessage message)
        {
            double fontSize = GetBreakMessageFontSize(message.Text);
            BreakMessageText.Text = message.Text;
            BreakMessageText.FontSize = fontSize;
            BreakMessageText.LineHeight = fontSize * 1.08;
            SetBreakMessageIcon(message);
        }

        private void SetBreakMessageIcon(BreakMessage message)
        {
            if (!string.IsNullOrWhiteSpace(message.IconImageUrl) &&
                breakMessageIconCache.TryGetImage(message.IconImageUrl, out ImageSource image))
            {
                BreakMessageIconBorder.Background = new ImageBrush(image)
                {
                    Stretch = Stretch.UniformToFill
                };
                BreakMessageIconText.Visibility = Visibility.Collapsed;
                return;
            }

            if (!string.IsNullOrWhiteSpace(message.IconImageUrl))
            {
                breakMessageIconCache.Preload(message);
            }

            BreakMessageIconText.Visibility = Visibility.Visible;
            BreakMessageIconText.Text = message.IconGlyph;
            BreakMessageIconText.FontSize = GetIconFontSize(message.IconGlyph);
            BreakMessageIconBorder.Background = CreateBrush(message.IconBackground);
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

        private static double GetIconFontSize(string iconGlyph)
        {
            return iconGlyph.Length switch
            {
                <= 1 => 25,
                2 => 22,
                _ => 18
            };
        }

        private static Brush CreateBrush(string color)
        {
            Brush brush = (Brush)new BrushConverter().ConvertFromString(color)!;
            brush.Freeze();
            return brush;
        }

        private static Brush CreateProgressFillBrush(double progress)
        {
            Color color = progress < 0.5
                ? InterpolateColor(ProgressEarlyColor, ProgressMiddleColor, progress / 0.5)
                : InterpolateColor(ProgressMiddleColor, ProgressLateColor, (progress - 0.5) / 0.5);

            SolidColorBrush brush = new(color);
            brush.Freeze();
            return brush;
        }

        private static Color InterpolateColor(Color start, Color end, double amount)
        {
            double clampedAmount = Math.Clamp(amount, 0, 1);
            return Color.FromRgb(
                InterpolateByte(start.R, end.R, clampedAmount),
                InterpolateByte(start.G, end.G, clampedAmount),
                InterpolateByte(start.B, end.B, clampedAmount));
        }

        private static byte InterpolateByte(byte start, byte end, double amount)
        {
            return (byte)Math.Round(start + ((end - start) * amount));
        }

        private void RenderProgressMarkers(LongBreakProgressState progressState)
        {
            LongBreakTickCanvas.Children.Clear();
            ProgressLabelCanvas.Children.Clear();

            AddEndpointLabel(progressState.StartHourLabel, 0, TextAlignment.Left);
            if (!HasEndMarker(progressState))
            {
                AddEndpointLabel(progressState.EndHourLabel, ProgressBarWidth - ProgressLabelWidth, TextAlignment.Right);
            }

            foreach (LongBreakProgressMarker marker in progressState.LongBreakMarkers)
            {
                double centerX = marker.Position * ProgressBarWidth;
                bool isPast = marker.Position <= progressState.PeriodProgress;
                Rectangle tick = new()
                {
                    Width = ProgressMarkerWidth,
                    Height = ProgressMarkerHeight,
                    Fill = isPast ? ProgressPassedMarkerBrush : ProgressMarkerBrush
                };

                Canvas.SetLeft(tick, Math.Clamp(centerX - (ProgressMarkerWidth / 2), 0, ProgressBarWidth - ProgressMarkerWidth));
                Canvas.SetTop(tick, 2);
                LongBreakTickCanvas.Children.Add(tick);

                AddTickLabel(marker.HourLabel, centerX, isPast);
            }
        }

        private static bool HasEndMarker(LongBreakProgressState progressState)
        {
            foreach (LongBreakProgressMarker marker in progressState.LongBreakMarkers)
            {
                if (marker.HourLabel == progressState.EndHourLabel &&
                    marker.Position > 0.94)
                {
                    return true;
                }
            }

            return false;
        }

        private void AddEndpointLabel(string text, double left, TextAlignment textAlignment)
        {
            TextBlock label = CreateProgressLabel(text, ProgressEndpointLabelBrush, textAlignment);
            Canvas.SetLeft(label, left);
            Canvas.SetTop(label, 0);
            ProgressLabelCanvas.Children.Add(label);
        }

        private void AddTickLabel(string text, double centerX, bool isPast)
        {
            Brush foreground = isPast ? ProgressPassedTickLabelBrush : ProgressTickLabelBrush;
            TextBlock label = CreateProgressLabel(text, foreground, TextAlignment.Center);
            Canvas.SetLeft(label, GetCenteredProgressLabelLeft(centerX));
            Canvas.SetTop(label, 0);
            ProgressLabelCanvas.Children.Add(label);
        }

        internal static double GetCenteredProgressLabelLeft(double centerX)
        {
            return centerX - (ProgressLabelWidth / 2);
        }

        private static TextBlock CreateProgressLabel(string text, Brush foreground, TextAlignment textAlignment)
        {
            return new TextBlock
            {
                Width = ProgressLabelWidth,
                FontSize = 13,
                Foreground = foreground,
                Text = text,
                TextAlignment = textAlignment
            };
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
