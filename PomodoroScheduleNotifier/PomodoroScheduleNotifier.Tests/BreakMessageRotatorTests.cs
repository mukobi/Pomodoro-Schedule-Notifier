using System;
using System.Collections.Generic;
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

        [Fact]
        public void StandardMessages_UseOneWayOutForKinoLoy()
        {
            Assert.Contains(
                BreakMessageRotator.StandardMessages,
                message => message.Text == "one way out");
            Assert.DoesNotContain(
                BreakMessageRotator.StandardMessages,
                message => message.Text == "i can't swim");
        }

        [Fact]
        public void StandardReferenceMessages_HaveUniqueImageUrls()
        {
            HashSet<string> imageUrls = new();

            foreach (BreakMessage message in BreakMessageRotator.StandardMessages)
            {
                if (string.IsNullOrWhiteSpace(message.IconImageUrl))
                {
                    continue;
                }

                Assert.True(imageUrls.Add(message.IconImageUrl), $"Duplicate image URL: {message.IconImageUrl}");
            }
        }

        [Fact]
        public void StandardReferenceMessages_HaveUniqueImageFiles()
        {
            HashSet<string> imageFiles = new(StringComparer.OrdinalIgnoreCase);

            foreach (BreakMessage message in BreakMessageRotator.StandardMessages)
            {
                if (string.IsNullOrWhiteSpace(message.IconImageUrl))
                {
                    continue;
                }

                string imageFile = GetImageFileName(message.IconImageUrl);

                Assert.True(imageFiles.Add(imageFile), $"Duplicate image file: {imageFile}");
            }
        }

        [Fact]
        public void StandardMessages_AvoidKnownWeakReferenceImages()
        {
            Assert.Contains(
                BreakMessageRotator.StandardMessages,
                message => message.Text == "determination");
            Assert.Contains(
                BreakMessageRotator.StandardMessages,
                message => message.Text == "but it refused");
            Assert.Contains(
                BreakMessageRotator.StandardMessages,
                message => message.Text == "despite everything");
            Assert.Contains(
                BreakMessageRotator.StandardMessages,
                message => message.Text == "hope is something you give yourself");
            Assert.Contains(
                BreakMessageRotator.StandardMessages,
                message => message.Text == "you're perfect" &&
                    (message.IconImageUrl ?? string.Empty).Contains("Silco_Alt"));
            Assert.Contains(
                BreakMessageRotator.StandardMessages,
                message => message.Text == "in pursuit of great" &&
                    message.IconFocusY < 0.4);
            Assert.Contains(
                BreakMessageRotator.StandardMessages,
                message => message.Text == "think, mark!" &&
                    (message.IconImageUrl ?? string.Empty).Contains("Omni_Man_and_Mark_at_duty"));
            Assert.Contains(
                BreakMessageRotator.StandardMessages,
                message => message.Text == "do or do not. there is no try." &&
                    (message.IconImageUrl ?? string.Empty).Contains("YodaForceLift"));
            Assert.Contains(
                BreakMessageRotator.StandardMessages,
                message => message.Text == "hello there" &&
                    (message.IconImageUrl ?? string.Empty).Contains("Kenobi_faces_Grievous"));

            Assert.DoesNotContain(
                BreakMessageRotator.StandardMessages,
                message => message.Text == "leaves from the vine");
            Assert.DoesNotContain(
                BreakMessageRotator.StandardMessages,
                message => message.Text == "cooldown");
            Assert.DoesNotContain(
                BreakMessageRotator.StandardMessages,
                message => message.Text == "stay determined");
            Assert.DoesNotContain(
                BreakMessageRotator.StandardMessages,
                message => (message.IconImageUrl ?? string.Empty).Contains("TFA"));
            Assert.DoesNotContain(
                BreakMessageRotator.StandardMessages,
                message => (message.IconImageUrl ?? string.Empty).Contains("blanc_et_noir"));
            Assert.DoesNotContain(
                BreakMessageRotator.StandardMessages,
                message => (message.IconImageUrl ?? string.Empty).Contains("Silco2_Arcane_Render"));
            Assert.DoesNotContain(
                BreakMessageRotator.StandardMessages,
                message => (message.IconImageUrl ?? string.Empty).Contains("Silco_Headshot"));
            Assert.DoesNotContain(
                BreakMessageRotator.StandardMessages,
                message => (message.IconImageUrl ?? string.Empty).Contains("Silco_Face_Model"));
            Assert.DoesNotContain(
                BreakMessageRotator.StandardMessages,
                message => (message.IconImageUrl ?? string.Empty).Contains("Yoda_SWSB"));
            Assert.DoesNotContain(
                BreakMessageRotator.StandardMessages,
                message => (message.IconImageUrl ?? string.Empty).Contains("ObiWanHS"));
        }

        private static string GetImageFileName(string imageUrl)
        {
            string imagePath = imageUrl.Split('?')[0];
            const string revisionSegment = "/revision/latest";
            int revisionIndex = imagePath.IndexOf(revisionSegment, StringComparison.OrdinalIgnoreCase);
            if (revisionIndex >= 0)
            {
                imagePath = imagePath.Substring(0, revisionIndex);
            }

            int slashIndex = imagePath.LastIndexOf('/');
            return Uri.UnescapeDataString(imagePath.Substring(slashIndex + 1));
        }
    }
}
