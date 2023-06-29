using Felipe.YoutubeExtractor.Core.Models;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Felipe.YoutubeExtractor.Services
{
    public static class ConfigService
    {
        private static readonly string _path = "config.json";

        public static async Task<OptionsModel> CreateConfigFile()
        {
            var options = new OptionsModel();

            await WriteConfigFile(options);

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
            return !File.Exists(OptionsModel.GetYtDlpDefaultFolder()) || !File.Exists(OptionsModel.GetFfmpegDefaultFile());
        }

        public static async Task<(OptionsModel config, bool createdConfig)> StartupConfig()
        {
            var createdConfig = false;
            CreateOutputFolderIfNotExists();

            if (!File.Exists(_path))
            {
                await CreateConfigFile();

                createdConfig = true;
            }

            var config = await LoadConfig();

            if (config == null)
            {
                return (await CreateConfigFile(), true);
            }

            return (config, createdConfig);
        }

        private static async Task<OptionsModel?> LoadConfig()
        {
            var configString = await File.ReadAllTextAsync(_path);

            var config = JsonSerializer.Deserialize<OptionsModel>(configString);

            return config;
        }

        private static async Task WriteConfigFile(OptionsModel options)
        {
            var serializer = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            await File.WriteAllTextAsync(_path, JsonSerializer.Serialize(options, options: serializer));
        }

        public static async Task UpdateConfig(OptionsModel options)
        {
            await WriteConfigFile(options);
        }
    }
}
