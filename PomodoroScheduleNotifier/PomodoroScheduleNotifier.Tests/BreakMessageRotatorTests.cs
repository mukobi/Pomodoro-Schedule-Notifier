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

        [Fact]
        public void StandardReferenceMessages_HaveImageUrls()
        {
            const int genericMessageCount = 12;

            for (int i = genericMessageCount; i < BreakMessageRotator.StandardMessages.Count; i++)
            {
                BreakMessage message = BreakMessageRotator.StandardMessages[i];

                Assert.False(string.IsNullOrWhiteSpace(message.IconImageUrl));
                Assert.StartsWith("https://", message.IconImageUrl);
            }
        }

        [Fact]
        public void StandardMessages_HaveValidIconFocus()
        {
            foreach (BreakMessage message in BreakMessageRotator.StandardMessages)
            {
                Assert.InRange(message.IconFocusX, 0, 1);
                Assert.InRange(message.IconFocusY, 0, 1);
            }
        }
    }
}
