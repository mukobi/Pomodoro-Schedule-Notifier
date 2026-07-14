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
        private const double ProgressBarWidth = 560;
        private const double ProgressMarkerWidth = 4;
        private const double ProgressMarkerHeight = 34;
        private const double ProgressLabelWidth = 44;
        private const double ArtworkBackgroundHeight = 720;
        private const double ArtworkMinAspectRatio = 1.0;
        private const double ArtworkMaxAspectRatio = 16.0 / 9.0;
        private const double BreakMessageTextHorizontalInset = 90;
        private const double BreakMessageMaxTextWidthCap = 1050;
        private const double BreakMessageMaxTextHeight = 180;
        private const double BreakMessageMaxFontSize = 92;
        private const double BreakMessageMinFontSize = 36;
        private const double BreakMessageFontSizeStep = 2;
        private static readonly Color ProgressEarlyColor = Color.FromRgb(0x4E, 0x83, 0x78);
        private static readonly Color ProgressMiddleColor = Color.FromRgb(0xD8, 0xC2, 0x4B);
        private static readonly Color ProgressLateColor = Color.FromRgb(0xB8, 0x62, 0x48);
        private static readonly Brush ProgressMarkerBrush = CreateBrush("#F4F1E8");
        private static readonly Brush ProgressPassedMarkerBrush = CreateBrush("#5D625C");
        private static readonly Brush ProgressTickLabelBrush = CreateBrush("#FFFFFF");
        private static readonly Brush ProgressPassedTickLabelBrush = CreateBrush("#D0D4CC");
        private static readonly Brush ProgressEndpointLabelBrush = CreateBrush("#FFFFFF");
        private static readonly Brush ProgressLabelOutlineBrush = CreateBrush("#F0000000");
        private readonly BreakMessageRotator breakMessageRotator = new();
        private readonly BreakMessageIconCache breakMessageIconCache = BreakMessageIconCache.Shared;
        private readonly StretchPromptRotator stretchPromptRotator = new();
        private double artworkBackgroundWidth = ArtworkBackgroundHeight;
        private double breakMessageMaxTextWidth = ArtworkBackgroundHeight - BreakMessageTextHorizontalInset;
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
            StretchTargetText.Text = stretchPromptRotator.Next().ToUpperInvariant();
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
            PhaseText.Text = phaseState.Phase == CyclePhase.LongBreak ? "LONG BREAK" : "SHORT BREAK";

            LongBreakProgressState progressState = LongBreakProgress.GetState(nowLocal, phaseState);
            PeriodProgressFillScale.ScaleX = progressState.PeriodProgress;
            PeriodProgressFill.Fill = CreateProgressFillBrush(progressState.PeriodProgress);
            RenderProgressMarkers(progressState);
        }

        private void SetBreakMessage(BreakMessage message)
        {
            ApplyArtworkAspectRatio(message.ArtworkAspectRatio);
            BreakMessageText.Text = message.Text.ToUpperInvariant();
            FitBreakMessageText();
            SetBreakMessageArtwork(message);
        }

        private void ApplyArtworkAspectRatio(double aspectRatio)
        {
            double clampedAspectRatio = double.IsNaN(aspectRatio)
                ? ArtworkMinAspectRatio
                : Math.Clamp(aspectRatio, ArtworkMinAspectRatio, ArtworkMaxAspectRatio);

            artworkBackgroundWidth = Math.Round(ArtworkBackgroundHeight * clampedAspectRatio);
            breakMessageMaxTextWidth = Math.Min(
                artworkBackgroundWidth - BreakMessageTextHorizontalInset,
                BreakMessageMaxTextWidthCap);

            Width = artworkBackgroundWidth;
            WindowShell.MinHeight = ArtworkBackgroundHeight;
            BreakMessageText.MaxWidth = breakMessageMaxTextWidth;
        }

        private void SetBreakMessageArtwork(BreakMessage message)
        {
            Brush accentBrush = CreateBrush(message.IconBackground);
            BreakMessageAccentLine.Fill = accentBrush;

            if (!string.IsNullOrWhiteSpace(message.IconImageUrl) &&
                breakMessageIconCache.TryGetImage(message.IconImageUrl, out ImageSource image))
            {
                BreakArtworkBackground.Background = CreateBackgroundImageBrush(image, message);
                BreakMessageIconText.Visibility = Visibility.Collapsed;
                return;
            }

            if (!string.IsNullOrWhiteSpace(message.IconImageUrl))
            {
                breakMessageIconCache.Preload(message);
            }

            BreakMessageIconText.Visibility = Visibility.Visible;
            BreakMessageIconText.Text = message.IconGlyph.ToUpperInvariant();
            BreakMessageIconText.FontSize = GetHeroGlyphFontSize(message.IconGlyph);
            BreakArtworkBackground.Background = accentBrush;
        }

        private static ImageBrush CreateBackgroundImageBrush(ImageSource image, BreakMessage message)
        {
            return new ImageBrush(image)
            {
                Stretch = Stretch.Fill,
                ViewboxUnits = BrushMappingMode.RelativeToBoundingBox,
                Viewbox = GetImageViewbox(
                    image.Width,
                    image.Height,
                    Math.Round(ArtworkBackgroundHeight * Math.Clamp(message.ArtworkAspectRatio, ArtworkMinAspectRatio, ArtworkMaxAspectRatio)),
                    ArtworkBackgroundHeight,
                    message.IconFocusX,
                    message.IconFocusY)
            };
        }

        internal static Rect GetImageViewbox(
            double imageWidth,
            double imageHeight,
            double targetWidth,
            double targetHeight,
            double focusX,
            double focusY)
        {
            if (imageWidth <= 0 ||
                imageHeight <= 0 ||
                targetWidth <= 0 ||
                targetHeight <= 0 ||
                double.IsNaN(imageWidth) ||
                double.IsNaN(imageHeight) ||
                double.IsNaN(targetWidth) ||
                double.IsNaN(targetHeight))
            {
                return new Rect(0, 0, 1, 1);
            }

            double clampedFocusX = ClampUnit(focusX);
            double clampedFocusY = ClampUnit(focusY);
            double imageAspectRatio = imageWidth / imageHeight;
            double targetAspectRatio = targetWidth / targetHeight;

            if (imageAspectRatio > targetAspectRatio)
            {
                double viewboxWidth = targetAspectRatio / imageAspectRatio;
                double left = Math.Clamp(clampedFocusX - (viewboxWidth / 2), 0, 1 - viewboxWidth);
                return new Rect(left, 0, viewboxWidth, 1);
            }

            if (imageAspectRatio < targetAspectRatio)
            {
                double viewboxHeight = imageAspectRatio / targetAspectRatio;
                double top = Math.Clamp(clampedFocusY - (viewboxHeight / 2), 0, 1 - viewboxHeight);
                return new Rect(0, top, 1, viewboxHeight);
            }

            return new Rect(0, 0, 1, 1);
        }

        private static double ClampUnit(double value)
        {
            return double.IsNaN(value) ? 0.5 : Math.Clamp(value, 0, 1);
        }

        private void FitBreakMessageText()
        {
            for (double fontSize = BreakMessageMaxFontSize; fontSize >= BreakMessageMinFontSize; fontSize -= BreakMessageFontSizeStep)
            {
                ApplyBreakMessageFontSize(fontSize);
                BreakMessageText.Measure(new Size(breakMessageMaxTextWidth, double.PositiveInfinity));
                if (BreakMessageText.DesiredSize.Height <= BreakMessageMaxTextHeight)
                {
                    return;
                }
            }

            ApplyBreakMessageFontSize(BreakMessageMinFontSize);
        }

        private void ApplyBreakMessageFontSize(double fontSize)
        {
            BreakMessageText.FontSize = fontSize;
            BreakMessageText.LineHeight = fontSize * 1.08;
        }

        private static double GetHeroGlyphFontSize(string iconGlyph)
        {
            return iconGlyph.Length switch
            {
                <= 1 => 210,
                2 => 170,
                3 => 132,
                _ => 112
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
                double centerX = GetProgressMarkerCenter(marker, progressState);
                bool isPast = marker.Position <= progressState.PeriodProgress;
                Rectangle tick = new()
                {
                    Width = ProgressMarkerWidth,
                    Height = ProgressMarkerHeight,
                    Fill = isPast ? ProgressPassedMarkerBrush : ProgressMarkerBrush
                };

                Canvas.SetLeft(
                    tick,
                    Math.Clamp(
                        centerX - (ProgressMarkerWidth / 2),
                        -(ProgressMarkerWidth / 2),
                        ProgressBarWidth - (ProgressMarkerWidth / 2)));
                Canvas.SetTop(tick, 2);
                LongBreakTickCanvas.Children.Add(tick);

                AddTickLabel(marker.HourLabel, centerX, isPast);
            }
        }

        internal static double GetProgressMarkerCenter(LongBreakProgressMarker marker, LongBreakProgressState progressState)
        {
            if (marker.HourLabel == progressState.EndHourLabel &&
                marker.Position > 0.94)
            {
                return ProgressBarWidth;
            }

            return marker.Position * ProgressBarWidth;
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
            FrameworkElement label = CreateProgressLabel(text, ProgressEndpointLabelBrush, textAlignment);
            Canvas.SetLeft(label, left);
            Canvas.SetTop(label, 0);
            ProgressLabelCanvas.Children.Add(label);
        }

        private void AddTickLabel(string text, double centerX, bool isPast)
        {
            Brush foreground = isPast ? ProgressPassedTickLabelBrush : ProgressTickLabelBrush;
            FrameworkElement label = CreateProgressLabel(text, foreground, TextAlignment.Center);
            Canvas.SetLeft(label, GetCenteredProgressLabelLeft(centerX));
            Canvas.SetTop(label, 0);
            ProgressLabelCanvas.Children.Add(label);
        }

        internal static double GetCenteredProgressLabelLeft(double centerX)
        {
            return centerX - (ProgressLabelWidth / 2);
        }

        private static FrameworkElement CreateProgressLabel(string text, Brush foreground, TextAlignment textAlignment)
        {
            Grid label = new()
            {
                Width = ProgressLabelWidth,
                Height = 18,
                SnapsToDevicePixels = true
            };

            AddProgressLabelText(label, text, ProgressLabelOutlineBrush, textAlignment, -1, 0);
            AddProgressLabelText(label, text, ProgressLabelOutlineBrush, textAlignment, 1, 0);
            AddProgressLabelText(label, text, ProgressLabelOutlineBrush, textAlignment, 0, -1);
            AddProgressLabelText(label, text, ProgressLabelOutlineBrush, textAlignment, 0, 1);
            AddProgressLabelText(label, text, foreground, textAlignment, 0, 0);
            return label;
        }

        private static void AddProgressLabelText(
            Grid label,
            string text,
            Brush foreground,
            TextAlignment textAlignment,
            double offsetX,
            double offsetY)
        {
            TextBlock textBlock = new()
            {
                Width = ProgressLabelWidth,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = foreground,
                Text = text,
                TextAlignment = textAlignment,
                RenderTransform = new TranslateTransform(offsetX, offsetY)
            };

            label.Children.Add(textBlock);
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
