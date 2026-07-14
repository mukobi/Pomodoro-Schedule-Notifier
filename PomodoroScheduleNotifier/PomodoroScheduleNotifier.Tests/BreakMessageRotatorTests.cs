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
                Assert.InRange(message.ArtworkAspectRatio, 1.0, 16.0 / 9.0);
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
                    (message.IconImageUrl ?? string.Empty).Contains("Nolan_yells_at_Mark") &&
                    message.ArtworkAspectRatio > 1.7);
            Assert.Contains(
                BreakMessageRotator.StandardMessages,
                message => message.Text == "are you sure?" &&
                    (message.IconImageUrl ?? string.Empty).Contains("Are-you-sure-Invincible"));
            Assert.Contains(
                BreakMessageRotator.StandardMessages,
                message => message.Text == "bang" &&
                    (message.IconImageUrl ?? string.Empty).Contains("Spike_Spiegel_bang"));
            Assert.Contains(
                BreakMessageRotator.StandardMessages,
                message => message.Text == "do or do not. there is no try." &&
                    (message.IconImageUrl ?? string.Empty).Contains("YodaForceLift"));
            Assert.Contains(
                BreakMessageRotator.StandardMessages,
                message => message.Text == "hello there" &&
                    (message.IconImageUrl ?? string.Empty).Contains("Kenobi_faces_Grievous"));
            Assert.Contains(
                BreakMessageRotator.StandardMessages,
                message => message.Text == "this is where the fun begins" &&
                    message.ArtworkAspectRatio > 1);
            Assert.Contains(
                BreakMessageRotator.StandardMessages,
                message => message.Text == "one way out" &&
                    message.ArtworkAspectRatio > 1.7);
            Assert.Contains(
                BreakMessageRotator.StandardMessages,
                message => message.Text == "what do i sacrifice? everything.");
            Assert.Contains(
                BreakMessageRotator.StandardMessages,
                message => message.Text == "eater of worlds has awoken!");
            Assert.Contains(
                BreakMessageRotator.StandardMessages,
                message => message.Text == "skeletron has awoken!");
            Assert.Contains(
                BreakMessageRotator.StandardMessages,
                message => message.Text == "eye of cthulhu has awoken!");
            Assert.Contains(
                BreakMessageRotator.StandardMessages,
                message => message.Text == "wall of flesh has awoken!");
            Assert.Contains(
                BreakMessageRotator.StandardMessages,
                message => message.Text == "the baby" &&
                    (message.IconImageUrl ?? string.Empty).Contains("Baby_Metroid"));
            Assert.Contains(
                BreakMessageRotator.StandardMessages,
                message => message.Text == "we gon' be alright" &&
                    (message.IconImageUrl ?? string.Empty).Contains("Z-48u_uWMHY"));
            Assert.Contains(
                BreakMessageRotator.StandardMessages,
                message => message.Text == "you're my soda pop" &&
                    (message.IconImageUrl ?? string.Empty).Contains("nflximg"));
            Assert.Contains(
                BreakMessageRotator.StandardMessages,
                message => message.Text == "lisan al-gaib" &&
                    message.ArtworkAspectRatio > 1.7);

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
                message => message.Text == "soda pop");
            Assert.DoesNotContain(
                BreakMessageRotator.StandardMessages,
                message => message.Text == "magic mirror");
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
            Assert.DoesNotContain(
                BreakMessageRotator.StandardMessages,
                message => (message.IconImageUrl ?? string.Empty).Contains("Omni_Man_and_Mark_at_duty"));
            Assert.DoesNotContain(
                BreakMessageRotator.StandardMessages,
                message => (message.IconImageUrl ?? string.Empty).Contains("NolanGrayson-render"));
            Assert.DoesNotContain(
                BreakMessageRotator.StandardMessages,
                message => (message.IconImageUrl ?? string.Empty).Contains("Saja_boys_demon"));
            Assert.DoesNotContain(
                BreakMessageRotator.StandardMessages,
                message => (message.IconImageUrl ?? string.Empty).Contains("Rumi_Portrait"));
            Assert.DoesNotContain(
                BreakMessageRotator.StandardMessages,
                message => (message.IconImageUrl ?? string.Empty).Contains("Kendrick_Lamar_2025"));
            Assert.DoesNotContain(
                BreakMessageRotator.StandardMessages,
                message => (message.IconImageUrl ?? string.Empty).Contains("MD_Samus_Infected"));
        }

        private static string GetImageFileName(string imageUrl)
        {
            Uri uri = new(imageUrl);
            if (string.Equals(uri.Host, "i.ytimg.com", StringComparison.OrdinalIgnoreCase))
            {
                string[] segments = uri.AbsolutePath.Trim('/').Split('/');
                if (segments.Length >= 3)
                {
                    return $"{segments[^2]}/{segments[^1]}";
                }
            }

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
