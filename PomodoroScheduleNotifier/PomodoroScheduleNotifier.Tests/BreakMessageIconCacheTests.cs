using System;
using System.IO;
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
    }
}
