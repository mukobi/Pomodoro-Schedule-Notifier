using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using PomodoroScheduleNotifier;
using Xunit;

namespace PomodoroScheduleNotifier.Tests
{
    public class BreakMessageIconCacheTests
    {
        [Fact]
        public void Clear_RemovesCachedFiles()
        {
            string cacheDirectory = Path.Combine(
                Path.GetTempPath(),
                "PomodoroScheduleNotifierTests",
                Guid.NewGuid().ToString("N"));
            string cacheFile = Path.Combine(cacheDirectory, "cached.img");

            try
            {
                Directory.CreateDirectory(cacheDirectory);
                File.WriteAllText(cacheFile, "cached");

                BreakMessageIconCache cache = new(cacheDirectory);
                cache.Clear();

                Assert.False(File.Exists(cacheFile));
            }
            finally
            {
                if (Directory.Exists(cacheDirectory))
                {
                    Directory.Delete(cacheDirectory, true);
                }
            }
        }

        [Fact]
        public void TryGetImage_RetriesAfterFailedLoad()
        {
            string cacheDirectory = Path.Combine(
                Path.GetTempPath(),
                "PomodoroScheduleNotifierTests",
                Guid.NewGuid().ToString("N"));
            int loadCount = 0;
            BreakMessageIconCache cache = new(
                cacheDirectory,
                _ =>
                {
                    loadCount++;
                    return Task.FromResult<ImageSource?>(null);
                });

            try
            {
                Assert.False(cache.TryGetImage("https://example.com/icon.png", out _));
                Assert.False(cache.TryGetImage("https://example.com/icon.png", out _));

                Assert.Equal(2, loadCount);
            }
            finally
            {
                if (Directory.Exists(cacheDirectory))
                {
                    Directory.Delete(cacheDirectory, true);
                }
            }
        }
    }
}
