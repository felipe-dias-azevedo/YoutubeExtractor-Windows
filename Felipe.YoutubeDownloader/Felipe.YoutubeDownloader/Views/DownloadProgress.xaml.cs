using Felipe.YoutubeDownloader.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;
using static System.Windows.Forms.Design.AxImporter;

namespace Felipe.YoutubeDownloader
{
    /// <summary>
    /// Lógica interna para DownloadProgress.xaml
    /// </summary>
    public partial class DownloadProgress : Window
    {
        private CancellationTokenSource _cts;

        private readonly VideoOptionsModel? _videoOptions;

        public DownloadProgress(bool progressVisible = false, string labelContent = "Downloading...")
        {
            _cts = new CancellationTokenSource();

            InitializeComponent();

            DownloadProgressBar.Visibility = progressVisible ? Visibility.Visible : Visibility.Hidden;
            DownloadLabel.Content = labelContent;
        }

        public DownloadProgress(VideoOptionsModel videoOptionsModel)
        {
            _videoOptions = videoOptionsModel;
            _cts = new CancellationTokenSource();

            InitializeComponent();
        }

        public DownloadProgress(bool downloadDependencies = true)
        {
            _cts = new CancellationTokenSource();

            InitializeComponent();

            if (downloadDependencies)
            {
                StartDependenciesDownload();
            }
        }

        public async Task StartDependenciesDownload()
        {
            Show();

            try
            {
                var progressYtDlp = new Progress<float>(p =>
                {
                    var percent = p * 50;
                    DownloadProgressLabel.Content = string.Format("{0:0.0}%", percent);
                    DownloadProgressBar.Value = p * 0.5;
                });

                var progressFfmpeg = new Progress<float>(p =>
                {
                    var percent = (p * 50) + 50;
                    DownloadProgressLabel.Content = string.Format("{0:0.0}%", percent);
                    DownloadProgressBar.Value = (p * 0.5) + 0.5;
                });

                DownloadLabel.Content = "Downloading yt-dlp...";
                await DownloadService.DownloadYtDlp(cancellationToken: _cts.Token, progress: progressYtDlp);

                DownloadLabel.Content = "Downloading ffmpeg...";
                await DownloadService.DownloadFFmpeg(cancellationToken: _cts.Token, progress: progressFfmpeg);

                DownloadFinished();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\nDownload them again on settings.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                Close();

                return;
            }
        }

        public async Task StartYtDlpDownload()
        {
            Show();

            try
            {
                var progress = new Progress<float>(p =>
                {
                    var percent = p * 100;
                    DownloadProgressLabel.Content = string.Format("{0:0.0}%", percent);
                    DownloadProgressBar.Value = p;
                });

                DownloadLabel.Content = "Downloading...";
                await DownloadService.DownloadYtDlp(cancellationToken: _cts.Token, progress: progress);

                DownloadFinished();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                Close();

                return;
            }
        }

        public async Task StartFfmpegDownload()
        {
            Show();

            try
            {
                var progress = new Progress<float>(p =>
                {
                    var percent = p * 100;
                    DownloadProgressLabel.Content = string.Format("{0:0.0}%", percent);
                    DownloadProgressBar.Value = p;
                });

                DownloadLabel.Content = "Downloading...";
                await DownloadService.DownloadFFmpeg(cancellationToken: _cts.Token, progress: progress);

                DownloadFinished();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                Close();

                return;
            }
        }

        public async Task StartVideoDownload()
        {
            if (_videoOptions == null)
            {
                throw new InvalidOperationException("VideoOptions not informed for Video Download.");
            }

            Show();

            var youtube = new YoutubeService(_videoOptions);

            var progress = new Progress<YoutubeDLSharp.DownloadProgress>(p => 
            {
                var points = p.State != DownloadState.Error || p.State != DownloadState.Success ? "..." : "";
                var state = p.State == DownloadState.Error || p.State == DownloadState.Success ? "Loading" : p.State.ToString();
                var percent = p.Progress * 100;

                DownloadLabel.Content = $"{state}{points}";
                DownloadProgressLabel.Content = string.Format("{0:0.0}%", percent);
                DownloadProgressBar.Value = p.Progress;
            });

            var res = await youtube.Download(_cts.Token, progress);

            if (!res.Success)
            {
                MessageBox.Show(string.Join(" ", res.ErrorOutput), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                
                Close();

                return;
            }

            DownloadFinished();
        }

        private void DownloadFinished()
        {
            CancelBtn.IsEnabled = false;
            OkBtn.IsEnabled = true;
            OkBtn.IsDefault = true;
            DownloadLabel.Content = "Download Finished.";
            DownloadProgressLabel.Content = "100%";
            DownloadProgressBar.Value = 1;
        }

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            //DialogResult = true;
            Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            _cts.Cancel();

            Close();
            //DialogResult = false;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _cts.Cancel();
            //DialogResult = false;
        }
    }
}
