using Felipe.YoutubeExtractor.Extensions;
using Felipe.YoutubeExtractor.Services;
using Felipe.YoutubeExtractor.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

        private List<PlaylistVideoTableRowViewModel> _downloadQueue;

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

        private void UpdateQueueOnStatus(PlaylistVideoTableRowViewModel videoInQueue, PlaylistVideoTableRowStatus status)
        {
            videoInQueue.Status = status.ToString();
            DownloadQueueView.Items.Refresh();
        }

        private async Task<(string, string)> RunDownload(string id, Task<string> downloadTask)
        {
            var videoInQueue = _downloadQueue.FirstOrDefault(x => x.Id == id);
            
            if (videoInQueue == null)
            {
                throw new InvalidOperationException("Video not found on queue.");
            }

            try
            {
                UpdateQueueOnStatus(videoInQueue, PlaylistVideoTableRowStatus.DownloadInProgress);

                var result = await downloadTask;

                UpdateQueueOnStatus(videoInQueue, PlaylistVideoTableRowStatus.DownloadDone);

                return (result, id);
            } 
            catch
            {
                UpdateQueueOnStatus(videoInQueue, PlaylistVideoTableRowStatus.Error);

                throw;
            }
        }

        private async Task RunNormalize(string id, Task normalizeTask)
        {
            var videoInQueue = _downloadQueue.FirstOrDefault(x => x.Id == id);

            if (videoInQueue == null)
            {
                throw new InvalidOperationException("Video not found on queue.");
            }

            try
            {
                UpdateQueueOnStatus(videoInQueue, PlaylistVideoTableRowStatus.NormalizeInProgress);

                await normalizeTask;

                UpdateQueueOnStatus(videoInQueue, PlaylistVideoTableRowStatus.Done);
            }
            catch
            {
                UpdateQueueOnStatus(videoInQueue, PlaylistVideoTableRowStatus.Error);

                throw;
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

            try
            {
                if (_videoOptions.IsPlaylist)
                {
                    DownloadProgressBar.IsIndeterminate = true;
                    DownloadProgressLabel.Content = "";
                    DownloadLabel.Content = $"Fetching Playlist Videos...";
                    var videosIds = await youtube.GetVideoIds(_cts.Token);
                    var videosUrlId = videosIds.Select(id => YoutubeService.GetUrlFromId(id)).ToList();

                    var fetchingParallel = videosUrlId.Select(id => youtube.Fetch(id, _cts.Token));

                    DownloadLabel.Content = $"Fetching Playlist Videos Data...";
                    var videosData = await Task.WhenAll(fetchingParallel);

                    _downloadQueue = videosData.Select(x => new PlaylistVideoTableRowViewModel
                    {
                        Id = YoutubeService.GetUrlFromId(x.ID),
                        Name = x.Title,
                        Artist = x.Artist ?? x.Creator ?? x.Channel,
                        Status = PlaylistVideoTableRowStatus.InQueue.ToString(),
                    }).ToList();

                    DownloadQueueView.Visibility = Visibility.Visible;
                    DownloadQueueView.ItemsSource = _downloadQueue;
                    DownloadLabel.Content = $"Downloading Videos...";

                    var downloadingParallel = videosUrlId.Select(id => RunDownload(id, youtube.Download(id, _cts.Token)));

                    var downloads = await Task.WhenAll(downloadingParallel);

                    if (_videoOptions.NormalizeAudio)
                    {
                        DownloadLabel.Content = $"Normalizing Audio...";
                        var normalizingParallel = downloads.Select(filePathAndId => RunNormalize(filePathAndId.Item2, youtube.Normalize(filePathAndId.Item1, _cts.Token)));

                        await Task.WhenAll(normalizingParallel);
                    }

                    DownloadFinished();

                    return;
                }

                DownloadLabel.Content = $"Fetching Video's Data...";
                DownloadProgressBar.IsIndeterminate = true;
                var videoData = await youtube.Fetch(cancellationToken: _cts.Token);

                CurrentTitleLabel.Content = $"{videoData.Title} ~ {videoData.Artist ?? videoData.Creator ?? videoData.Channel}";
                CurrentTitleLabel.Visibility = Visibility.Visible;

                var downloadPath = await youtube.Download(cancellationToken: _cts.Token, progress: progress);

                if (_videoOptions.NormalizeAudio)
                {
                    DownloadLabel.Content = $"Normalizing Audio...";
                    DownloadProgressBar.IsIndeterminate = true;

                    await youtube.Normalize(downloadPath, _cts.Token);
                }

                DownloadFinished();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Join("\n", ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                Close();

                return;
            }
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
            AutoCloseCheckBox.IsEnabled = false;

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
