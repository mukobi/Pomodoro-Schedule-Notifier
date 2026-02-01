using System;
using System.IO;
using System.Text.Json;

namespace PomodoroScheduleNotifier
{
    public class UserSettings
    {
        public double VolumeDb { get; set; } = 0;

        public static UserSettings Load()
        {
            string path = GetSettingsPath();
            if (!File.Exists(path))
            {
                return new UserSettings();
            }

            try
            {
                string json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<UserSettings>(json) ?? new UserSettings();
            }
            catch
            {
                return new UserSettings();
            }
        }

        public static void Save(UserSettings settings)
        {
            string path = GetSettingsPath();
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        private static string GetSettingsPath()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "PomodoroScheduleNotifier", "settings.json");
        }
    }
}
