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
        public void GetIconViewbox_CropsWideImagesAroundFocus()
        {
            var result = BreakReminderWindow.GetIconViewbox(200, 100, 0.75, 0.5);

            Assert.Equal(0.5, result.X, 4);
            Assert.Equal(0, result.Y, 4);
            Assert.Equal(0.5, result.Width, 4);
            Assert.Equal(1, result.Height, 4);
        }

        [Fact]
        public void GetIconViewbox_CropsTallImagesAroundFocus()
        {
            var result = BreakReminderWindow.GetIconViewbox(100, 200, 0.5, 0.75);

            Assert.Equal(0, result.X, 4);
            Assert.Equal(0.5, result.Y, 4);
            Assert.Equal(1, result.Width, 4);
            Assert.Equal(0.5, result.Height, 4);
        }

        [Fact]
        public void GetIconViewbox_UsesFullSquareImage()
        {
            var result = BreakReminderWindow.GetIconViewbox(100, 100, 0.75, 0.75);

            Assert.Equal(0, result.X, 4);
            Assert.Equal(0, result.Y, 4);
            Assert.Equal(1, result.Width, 4);
            Assert.Equal(1, result.Height, 4);
        }
    }
}
