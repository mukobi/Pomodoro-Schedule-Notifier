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

            BreakMessage message = rotator.Next();

            Assert.False(string.IsNullOrWhiteSpace(message.Text));
            Assert.False(string.IsNullOrWhiteSpace(message.IconGlyph));
            Assert.False(string.IsNullOrWhiteSpace(message.IconBackground));
        }

        [Fact]
        public void Next_ReturnsLowerCaseMessages()
        {
            BreakMessageRotator rotator = new();

            for (int i = 0; i < 100; i++)
            {
                BreakMessage message = rotator.Next();

                Assert.Equal(message.Text.ToLowerInvariant(), message.Text);
            }
        }
    }
}
