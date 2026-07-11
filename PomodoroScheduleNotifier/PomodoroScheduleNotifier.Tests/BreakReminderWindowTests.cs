using PomodoroScheduleNotifier;
using Xunit;

namespace PomodoroScheduleNotifier.Tests
{
    public class BreakReminderWindowTests
    {
        [Fact]
        public void GetCenteredProgressLabelLeft_DoesNotClampEdgeTickLabels()
        {
            double markerCenterX = 535.0 / 540.0 * 500.0;

            double result = BreakReminderWindow.GetCenteredProgressLabelLeft(markerCenterX);

            Assert.Equal(markerCenterX - 22.0, result, 4);
        }

        [Fact]
        public void GetImageViewbox_CropsWideImagesToTargetAspectAroundFocus()
        {
            var result = BreakReminderWindow.GetImageViewbox(200, 100, 150, 100, 0.5, 0.5);

            Assert.Equal(0.125, result.X, 4);
            Assert.Equal(0, result.Y, 4);
            Assert.Equal(0.75, result.Width, 4);
            Assert.Equal(1, result.Height, 4);
        }

        [Fact]
        public void GetImageViewbox_CropsTallImagesToTargetAspectAroundFocus()
        {
            var result = BreakReminderWindow.GetImageViewbox(100, 200, 100, 100, 0.5, 0.75);

            Assert.Equal(0, result.X, 4);
            Assert.Equal(0.5, result.Y, 4);
            Assert.Equal(1, result.Width, 4);
            Assert.Equal(0.5, result.Height, 4);
        }

        [Fact]
        public void GetImageViewbox_UsesFullImageWhenAspectMatches()
        {
            var result = BreakReminderWindow.GetImageViewbox(160, 100, 320, 200, 0.75, 0.75);

            Assert.Equal(0, result.X, 4);
            Assert.Equal(0, result.Y, 4);
            Assert.Equal(1, result.Width, 4);
            Assert.Equal(1, result.Height, 4);
        }

        [Fact]
        public void GetImageViewbox_ClampsInvalidFocusToCenter()
        {
            var result = BreakReminderWindow.GetImageViewbox(200, 100, 150, 100, double.NaN, 0.5);

            Assert.Equal(0.125, result.X, 4);
            Assert.Equal(0, result.Y, 4);
            Assert.Equal(0.75, result.Width, 4);
            Assert.Equal(1, result.Height, 4);
        }
    }
}
