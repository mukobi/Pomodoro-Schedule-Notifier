using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PomodoroScheduleNotifier
{
    public sealed class BreakMessageIconCache
    {
        private static readonly HttpClient HttpClient = CreateHttpClient();

        private readonly ConcurrentDictionary<string, Task<ImageSource?>> imageTasks = new();
        private readonly string cacheDirectory;
        private readonly Func<string, Task<ImageSource?>> loadImageAsync;

        public static BreakMessageIconCache Shared { get; } = new();

        private BreakMessageIconCache()
            : this(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PomodoroScheduleNotifier",
                "QuoteIcons"))
        {
        }

        internal BreakMessageIconCache(
            string cacheDirectory,
            Func<string, Task<ImageSource?>>? loadImageAsync = null)
        {
            this.cacheDirectory = cacheDirectory;
            this.loadImageAsync = loadImageAsync ?? LoadImageAsync;
        }

        public void Preload(IEnumerable<BreakMessage> messages)
        {
            foreach (BreakMessage message in messages)
            {
                Preload(message);
            }
        }

        public void Preload(BreakMessage message)
        {
            if (!string.IsNullOrWhiteSpace(message.IconImageUrl))
            {
                _ = GetOrCreateTask(message.IconImageUrl);
            }
        }

        public void Clear()
        {
            imageTasks.Clear();
            if (!Directory.Exists(cacheDirectory))
            {
                return;
            }

            foreach (string cacheFile in Directory.EnumerateFiles(cacheDirectory))
            {
                TryDelete(cacheFile);
            }
        }

        public bool TryGetImage(string imageUrl, out ImageSource image)
        {
            image = null!;

            Task<ImageSource?> task = GetOrCreateTask(imageUrl);
            if (!task.IsCompletedSuccessfully)
            {
                return false;
            }

            if (task.Result == null)
            {
                TryRemoveTask(imageUrl, task);
                return false;
            }

            image = task.Result;
            return true;
        }

        private Task<ImageSource?> GetOrCreateTask(string imageUrl)
        {
            return imageTasks.GetOrAdd(imageUrl, CreateLoadTask);
        }

        private Task<ImageSource?> CreateLoadTask(string imageUrl)
        {
            Task<ImageSource?> task;
            try
            {
                task = loadImageAsync(imageUrl);
            }
            catch
            {
                task = Task.FromResult<ImageSource?>(null);
            }

            _ = RemoveFailedTaskAsync(imageUrl, task);
            return task;
        }

        private async Task RemoveFailedTaskAsync(string imageUrl, Task<ImageSource?> task)
        {
            try
            {
                ImageSource? image = await task.ConfigureAwait(false);
                if (image != null)
                {
                    return;
                }
            }
            catch
            {
            }

            TryRemoveTask(imageUrl, task);
        }

        private bool TryRemoveTask(string imageUrl, Task<ImageSource?> task)
        {
            return ((ICollection<KeyValuePair<string, Task<ImageSource?>>>)imageTasks)
                .Remove(new KeyValuePair<string, Task<ImageSource?>>(imageUrl, task));
        }

        private async Task<ImageSource?> LoadImageAsync(string imageUrl)
        {
            try
            {
                Directory.CreateDirectory(cacheDirectory);
                string cachePath = GetCachePath(imageUrl);

                if (File.Exists(cachePath))
                {
                    try
                    {
                        return LoadBitmap(cachePath);
                    }
                    catch
                    {
                        TryDelete(cachePath);
                    }
                }

                using HttpResponseMessage response = await HttpClient.GetAsync(imageUrl).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                byte[] bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                ImageSource image = LoadBitmap(bytes);
                await File.WriteAllBytesAsync(cachePath, bytes).ConfigureAwait(false);
                return image;
            }
            catch
            {
                return null;
            }
        }

        private string GetCachePath(string imageUrl)
        {
            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(imageUrl));
            StringBuilder builder = new(hash.Length * 2);
            foreach (byte value in hash)
            {
                builder.Append(value.ToString("x2"));
            }

            return Path.Combine(cacheDirectory, $"{builder}.img");
        }

        private static HttpClient CreateHttpClient()
        {
            HttpClient client = new()
            {
                Timeout = TimeSpan.FromSeconds(8)
            };
            client.DefaultRequestHeaders.UserAgent.TryParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) PomodoroScheduleNotifier/1.0");
            client.DefaultRequestHeaders.Accept.ParseAdd("image/png");
            client.DefaultRequestHeaders.Accept.ParseAdd("image/jpeg");
            client.DefaultRequestHeaders.Accept.ParseAdd("image/gif");
            return client;
        }

        private static ImageSource LoadBitmap(byte[] bytes)
        {
            using MemoryStream stream = new(bytes);
            return LoadBitmap(stream);
        }

        private static ImageSource LoadBitmap(string cachePath)
        {
            using FileStream stream = File.OpenRead(cachePath);
            return LoadBitmap(stream);
        }

        private static ImageSource LoadBitmap(Stream stream)
        {
            BitmapImage bitmap = new();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        private static void TryDelete(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch
            {
            }
        }
    }
}
