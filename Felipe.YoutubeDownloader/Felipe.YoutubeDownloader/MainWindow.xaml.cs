using Felipe.YoutubeDownloader.Views;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using YoutubeDLSharp.Options;

namespace Felipe.YoutubeDownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            var config = ConfigService.StartupConfig();

            InitializeComponent();

            AudioFormatComboBox.ItemsSource = Enum.GetValues(typeof(AudioConversionFormat));

            LoadConfig(config);
        }

        private void LoadConfig(OptionsModel config)
        {
            BestAudioCheckBox.IsChecked = config.BestAudio;
            MetadataCheckBox.IsChecked = config.Metadata;
            ThumbnailCheckBox.IsChecked = config.EmbedThumbnail;
            AudioFormatComboBox.SelectedItem = config.AudioFormat;
        }

        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var config = ConfigService.StartupConfig();

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
            var config = ConfigService.StartupConfig();

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

            if (!Regex.IsMatch(YoutubeUrlTextBox.Text, @"^(https?\:\/\/)?((www\.)?youtube\.com|youtu\.be)\/.+$"))
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
                //DownloadType = ,
                //Format = ,
                BestAudio = BestAudioCheckBox.IsChecked ?? true,
                IsPlaylist = IsPlaylistCheckBox.IsChecked ?? false,
                Metadata = MetadataCheckBox.IsChecked ?? true,
                EmbedThumbnail = ThumbnailCheckBox.IsChecked ?? false
            };

            ConfigService.UpdateConfig(videoOptions);

            var downloadDialog = new DownloadProgress(videoOptionsModel: videoOptions);
            try
            {
                await downloadDialog.StartVideoDownload();
            } 
            catch (TaskCanceledException) 
            {
                MessageBox.Show("Download cancelled.", "Cancelled", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
