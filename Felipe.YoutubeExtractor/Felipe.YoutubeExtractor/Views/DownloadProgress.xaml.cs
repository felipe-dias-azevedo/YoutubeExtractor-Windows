using Felipe.YoutubeExtractor.Extensions;
using Felipe.YoutubeExtractor.Services;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using YoutubeDLSharp;

namespace Felipe.YoutubeExtractor
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
                // FIXME: Start async and await for it until done to show main window
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

                DownloadLabel.Content = "Downloading FFmpeg...";
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

                DownloadLabel.Content = "Downloading yt-dlp...";
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

                DownloadLabel.Content = "Downloading FFmpeg...";
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
                var progressState = p.State == DownloadState.Error || p.State == DownloadState.Success;

                var state = progressState ? "Loading" : p.State.ToString().CapitalizedToSpaceJoined();
                var percent = p.Progress * 100;

                DownloadLabel.Content = $"{state}...";
                DownloadProgressBar.IsIndeterminate = p.State == DownloadState.PreProcessing || p.State == DownloadState.PostProcessing;

                if (!progressState && DownloadProgressBar.Value <= p.Progress)
                {
                    DownloadProgressLabel.Content = string.Format("{0:0.0}%", percent >= 99 ? 99 : percent);
                    DownloadProgressBar.Value = p.Progress;
                }
            });

            var res = await youtube.Download(_cts.Token, progress);

            if (!res.Success)
            {
                MessageBox.Show(string.Join("\n", res.ErrorOutput), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                
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
            DownloadProgressBar.IsIndeterminate = false;

            var autoClose = AutoCloseCheckBox.IsChecked ?? false;

            if (autoClose)
            {
                Close();
            }
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
