using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace Felipe.YoutubeExtractor
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

        public static bool ShouldAskDownload()
        {
            return File.Exists(OptionsModel.GetYtDlpDefaultFolder()) || File.Exists(OptionsModel.GetFfmpegDefaultFile());
        }

        public static OptionsModel StartupConfig(out bool createdConfig)
        {
            createdConfig = false;
            CreateOutputFolderIfNotExists();

            if (!File.Exists(_path))
            {
                CreateConfigFile();

                createdConfig = true;
            }

            var configString = File.ReadAllText(_path);

            var config = JsonSerializer.Deserialize<OptionsModel>(configString);

            if (config == null)
            {
                createdConfig = true;

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
