using PomodoroScheduleNotifier;
using Xunit;

namespace PomodoroScheduleNotifier.Tests
{
    public class BreakMessageRotatorTests
    {
        [Fact]
        public void Next_ReturnsNonEmptyMessage()
        {
            BreakMessageRotator rotator = new();

            string message = rotator.Next();

            Assert.False(string.IsNullOrWhiteSpace(message));
        }

        [Fact]
        public void Next_ReturnsLowerCaseMessages()
        {
            BreakMessageRotator rotator = new();

            for (int i = 0; i < 100; i++)
            {
                string message = rotator.Next();

                Assert.Equal(message.ToLowerInvariant(), message);
            }
        }
    }
}
