using Felipe.YoutubeExtractor.Extensions;
using Felipe.YoutubeExtractor.Services;
using Felipe.YoutubeExtractor.Views;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using YoutubeDLSharp.Options;

namespace Felipe.YoutubeExtractor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async Task DownloadDependencies()
        {
            var response = MessageBox.Show(
                "Would you like to automatically download and configure yt-dlp and ffmpeg to default folder?\n\n" +
                "NO - \"I already have them, and I will configure their path on settings.\"",
                "Download Dependencies",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (response != MessageBoxResult.Yes)
            {
                return;
            }

            var download = new DownloadProgress(progressVisible: true);

            try
            {
                var downloadTask = download.StartDependenciesDownload();
                download.ShowDialog();
                await downloadTask;
            }
            catch (TaskCanceledException)
            {
                MessageBox.Show("Download cancelled.", "Cancelled", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var (config, createdConfig) = await ConfigService.StartupConfig();

            AudioFormatComboBox.ItemsSource = Enum.GetValues(typeof(AudioConversionFormat));

            LoadConfig(config);

            if (createdConfig && ConfigService.ShouldAskDownload())
            {
                await DownloadDependencies();
            }
        }

        private void LoadConfig(OptionsModel config)
        {
            BestAudioCheckBox.IsChecked = config.BestAudio;
            MetadataCheckBox.IsChecked = config.Metadata;
            ThumbnailCheckBox.IsChecked = config.EmbedThumbnail;
            AudioFormatComboBox.SelectedItem = config.AudioFormat;
            NormalizeAudioCheckBox.IsChecked = config.NormalizeAudio;
            AutoCloseDoneMenuItem.IsChecked = config.AutoCloseWhenDone;
        }

        private async void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var (config, _) = await ConfigService.StartupConfig();

            var settings = new Settings(config);

            settings.ShowDialog();
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var about = new About();

            about.ShowDialog();
        }

        private void YoutubeUrlTextBox_Changed(object sender, RoutedEventArgs e)
        {
            IsPlaylistCheckBox.IsChecked = YoutubeUrlTextBox.Text.Contains("list=");
            YoutubeUrlTextBox.Text = YoutubeUrlTextBox.Text.RemoveAllWhitespaces();
        }

        private void AudioFormatComboBox_Changed(object sender, SelectionChangedEventArgs e)
        {
            var audioFormatSelected = (AudioConversionFormat) AudioFormatComboBox.SelectedValue;
            var thumbnailChecked = ThumbnailCheckBox.IsChecked ?? false;

            if (audioFormatSelected == AudioConversionFormat.Wav && thumbnailChecked)
            {
                MessageBox.Show("WAV not compatible with thumnbnail embedding.", "Unsupported", MessageBoxButton.OK, MessageBoxImage.Information);

                ThumbnailCheckBox.IsChecked = false;
            }
        }

        private async void DownloadBtn_Click(object sender, RoutedEventArgs e)
        {
            var (config, _) = await ConfigService.StartupConfig();

            if (string.IsNullOrEmpty(config.OutputPath) ||
                string.IsNullOrEmpty(YoutubeUrlTextBox.Text) ||
                string.IsNullOrEmpty(config.YtdlpPath) ||
                string.IsNullOrEmpty(config.FfmpegPath))
            {
                MessageBox.Show("Required information not informed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!FileService.ExistsFile(config.YtdlpPath))
            {
                MessageBox.Show("yt-dlp executable not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!FileService.ExistsFolder(config.FfmpegPath))
            {
                MessageBox.Show("ffmpeg folder path does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!YoutubeService.IsValidUrl(YoutubeUrlTextBox.Text))
            {
                MessageBox.Show("Youtube URL might not be valid.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            if (!FileService.ExistsFolder(config.OutputPath)) 
            {
                MessageBox.Show("Output folder path does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var videoOptions = new VideoOptionsModel
            {
                OutputPath = config.OutputPath,
                YtdlpPath = config.YtdlpPath,
                YoutubeUrl = YoutubeUrlTextBox.Text,
                FfmpegPath = config.FfmpegPath,
                AudioFormat = (AudioConversionFormat) AudioFormatComboBox.SelectedValue,
                NormalizeAudio = NormalizeAudioCheckBox.IsChecked ?? false,
                //DownloadType = ,
                //Format = ,
                BestAudio = BestAudioCheckBox.IsChecked ?? true,
                IsPlaylist = IsPlaylistCheckBox.IsChecked ?? false,
                Metadata = MetadataCheckBox.IsChecked ?? true,
                EmbedThumbnail = ThumbnailCheckBox.IsChecked ?? false,
                AutoCloseWhenDone = AutoCloseDoneMenuItem.IsChecked
            };

            await ConfigService.UpdateConfig(videoOptions);

            var downloadDialog = new DownloadProgress(videoOptionsModel: videoOptions);
            try
            {
                downloadDialog.Show();
                await downloadDialog.StartVideoDownload();
            } 
            catch (TaskCanceledException) 
            {
                MessageBox.Show("Download cancelled.", "Cancelled", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
