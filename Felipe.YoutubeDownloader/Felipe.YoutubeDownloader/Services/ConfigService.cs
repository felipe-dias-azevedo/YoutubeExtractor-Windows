using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace Felipe.YoutubeDownloader
{
    public static class ConfigService
    {
        private static readonly string _path = "config.json";

        public static OptionsModel CreateConfigFile()
        {
            var options = new OptionsModel();

            WriteConfigFile(options);

            return options;
        }

        private static void CreateOutputFolderIfNotExists()
        {
            var outputFolder = Path.Combine(Directory.GetCurrentDirectory(), "output");

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }
        }

        public static OptionsModel StartupConfig()
        {
            CreateOutputFolderIfNotExists();

            if (!File.Exists(_path))
            {
                CreateConfigFile();

                if (!File.Exists(OptionsModel.GetYtDlpDefaultFolder()) && !File.Exists(OptionsModel.GetFfmpegDefaultFile()))
                {
                    var response = MessageBox.Show(
                        "Would you like to automatically download and configure yt-dlp and ffmpeg to default folder?\n\n" +
                        "NO - \"I already have them, and I will configure their path on settings.\"",
                        "Download Dependencies",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (response == MessageBoxResult.Yes)
                    {
                        try
                        {
                            _ = new DownloadProgress(downloadDependencies: true);
                        }
                        catch (TaskCanceledException)
                        {
                            MessageBox.Show("Download cancelled.", "Cancelled", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            }

            var configString = File.ReadAllText(_path);

            var config = JsonSerializer.Deserialize<OptionsModel>(configString);

            if (config == null)
            {
                return CreateConfigFile();
            }

            return config;
        }

        private static void WriteConfigFile(OptionsModel options)
        {
            var serializer = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            File.WriteAllText(_path, JsonSerializer.Serialize(options, options: serializer));
        }

        public static void UpdateConfig(OptionsModel options) 
        {
            WriteConfigFile(options);
        }
    }
}
