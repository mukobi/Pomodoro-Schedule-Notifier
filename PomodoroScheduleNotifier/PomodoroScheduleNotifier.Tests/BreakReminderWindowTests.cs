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
    }
}
