using PomodoroScheduleNotifier;
using Xunit;

namespace PomodoroScheduleNotifier.Tests
{
    public class BreakReminderInterruptionDetectorTests
    {
        [Theory]
        [InlineData("microphone", "C:\\Users\\Gabe\\AppData\\Local\\Microsoft\\Teams\\current\\Teams.exe")]
        [InlineData("microphone", "msteams_8wekyb3d8bbwe")]
        [InlineData("microphone", "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe meet.google.com")]
        [InlineData("webcam", "C:\\Program Files\\Logitech\\Camera.exe")]
        [InlineData("graphicsCaptureProgrammatic", "C:\\Tools\\SomeScreenShareApp.exe")]
        [InlineData("graphicsCaptureWithoutBorder", "unknown packaged app")]
        public void ShouldDeferForCapability_DefersForActiveMeetingAndCaptureSignals(
            string capability,
            string identity)
        {
            CapabilityAccessRecord record = new(capability, identity, true);

            bool shouldDefer = BreakReminderInterruptionDetector.ShouldDeferForCapability(record, out string reason);

            Assert.True(shouldDefer);
            Assert.False(string.IsNullOrWhiteSpace(reason));
        }

        [Fact]
        public void ShouldDeferForCapability_IgnoresInactiveSignals()
        {
            CapabilityAccessRecord record = new("microphone", "msteams_8wekyb3d8bbwe", false);

            bool shouldDefer = BreakReminderInterruptionDetector.ShouldDeferForCapability(record, out _);

            Assert.False(shouldDefer);
        }

        [Fact]
        public void ShouldDeferForCapability_DefersForAnyActiveMicrophone()
        {
            CapabilityAccessRecord record = new("microphone", "C:\\Windows\\System32\\svchost.exe", true);

            bool shouldDefer = BreakReminderInterruptionDetector.ShouldDeferForCapability(record, out string reason);

            Assert.True(shouldDefer);
            Assert.Contains("microphone", reason);
        }

        [Fact]
        public void ShouldDeferForAudioSession_DefersForActiveTeamsAudio()
        {
            BreakReminderInterruptionDetector.AudioSessionRecord session = new(
                "ms-teams Microsoft Teams Meeting",
                true);

            bool shouldDefer = BreakReminderInterruptionDetector.ShouldDeferForAudioSession(session, out string reason);

            Assert.True(shouldDefer);
            Assert.Contains("audio", reason);
        }

        [Fact]
        public void ShouldDeferForAudioSession_IgnoresInactiveTeamsAudio()
        {
            BreakReminderInterruptionDetector.AudioSessionRecord session = new(
                "ms-teams Microsoft Teams Meeting",
                false);

            bool shouldDefer = BreakReminderInterruptionDetector.ShouldDeferForAudioSession(session, out _);

            Assert.False(shouldDefer);
        }

        [Fact]
        public void ShouldDeferForAudioSession_DefersForActiveMicrophoneCapture()
        {
            BreakReminderInterruptionDetector.AudioSessionRecord session = new(
                "Windows Voice Typing",
                true,
                IsMicrophoneCapture: true);

            bool shouldDefer = BreakReminderInterruptionDetector.ShouldDeferForAudioSession(session, out string reason);

            Assert.True(shouldDefer);
            Assert.Contains("microphone", reason);
        }
    }
}
