using Felipe.YoutubeExtractor.Extensions;
using Felipe.YoutubeExtractor.Services;
using Felipe.YoutubeExtractor.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
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

            AutoCloseCheckBox.IsChecked = _videoOptions.AutoCloseWhenDone;
        }

        public async Task StartDependenciesDownload()
        {
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
                CloseOnException(ex.Message + "\n\nDownload them again on settings.");
            }
        }

        public async Task StartYtDlpDownload()
        {
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
                CloseOnException(ex.Message);
            }
        }

        public async Task StartFfmpegDownload()
        {
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
                CloseOnException(ex.Message);
            }
        }

        private void UpdateQueueOnStatus(PlaylistVideoTableRowViewModel videoInQueue, PlaylistVideoTableRowStatus status)
        {
            videoInQueue.Status = status.ToString();
            DownloadQueueView.Items.Refresh();
        }

        private void UpdateProgressQueue()
        {
            var generalTotal = _downloadQueue.Count();
            var sum = _downloadQueue.Count(x => x.StatusEnum == PlaylistVideoTableRowStatus.Done);
            var percent = (double)sum / generalTotal;

            if (_videoOptions!.NormalizeAudio)
            {
                var totalNormalize = generalTotal * 2;
                var sumNormalize = (sum * 2) + _downloadQueue.Count(x => x.StatusEnum == PlaylistVideoTableRowStatus.DownloadDone);
                percent = (double)sumNormalize / totalNormalize;
            }

            DownloadProgressLabel.Content = $"{sum}/{generalTotal} - {(percent * 100):0.0}%";
            DownloadProgressBar.Value = percent;
        }

        private async Task RunDownloadAndNormalize(string id, YoutubeService youtubeService)
        {
            var videoInQueue = _downloadQueue.FirstOrDefault(x => x.Id == id) 
                ?? throw new InvalidOperationException("Video not found on queue.");

            UpdateProgressQueue();

            try
            {
                UpdateQueueOnStatus(videoInQueue, PlaylistVideoTableRowStatus.DownloadInProgress);

                var downloadFilePath = await youtubeService.Download(id, _cts.Token);

                UpdateQueueOnStatus(videoInQueue, _videoOptions!.NormalizeAudio ? PlaylistVideoTableRowStatus.DownloadDone : PlaylistVideoTableRowStatus.Done);
                UpdateProgressQueue();

                if (_videoOptions.NormalizeAudio)
                {
                    UpdateQueueOnStatus(videoInQueue, PlaylistVideoTableRowStatus.NormalizeInProgress);

                    await youtubeService.Normalize(downloadFilePath, _cts.Token);

                    UpdateQueueOnStatus(videoInQueue, PlaylistVideoTableRowStatus.Done);
                    UpdateProgressQueue();
                }
            }
            catch
            {
                UpdateQueueOnStatus(videoInQueue, PlaylistVideoTableRowStatus.Error);

                throw;
            }
        }

        private void SetProgressOnSingleVideoDownload(YoutubeDLSharp.DownloadProgress p)
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
        }

        public async Task StartVideoDownload()
        {
            if (_videoOptions == null)
            {
                throw new InvalidOperationException("VideoOptions not informed for Video Download.");
            }

            var youtube = new YoutubeService(_videoOptions);

            var progress = new Progress<YoutubeDLSharp.DownloadProgress>(p => SetProgressOnSingleVideoDownload(p));

            try
            {
                if (_videoOptions.IsPlaylist)
                {
                    DownloadProgressBar.IsIndeterminate = true;
                    DownloadProgressLabel.Content = "";

                    DownloadLabel.Content = $"Fetching Playlist's Data...";
                    var videoFetchData = await youtube.Fetch(cancellationToken: _cts.Token);

                    if (videoFetchData.ResultType == YoutubeDLSharp.Metadata.MetadataType.Video) 
                    {
                        var uri = new Uri(_videoOptions.YoutubeUrl);

                        var playlistId = HttpUtility.ParseQueryString(uri.Query).Get("list");

                        if (playlistId != null) 
                        {
                            var url = YoutubeService.GetPlaylistUrlFromId(playlistId);

                            var playlistFetchData = await youtube.Fetch(url, cancellationToken: _cts.Token);

                            CurrentTitleLabel.Content = playlistFetchData.Title;
                            CurrentTitleLabel.Visibility = Visibility.Visible;
                        }
                    }

                    if (videoFetchData.ResultType == YoutubeDLSharp.Metadata.MetadataType.Playlist)
                    {
                        CurrentTitleLabel.Content = videoFetchData.Title;
                        CurrentTitleLabel.Visibility = Visibility.Visible;
                    }

                    DownloadLabel.Content = $"Fetching Playlist Videos...";
                    var videosIds = await youtube.GetVideoIdsSafe(_cts.Token);
                    var videosUrlId = videosIds.Select(id => YoutubeService.GetVideoUrlFromId(id)).ToList();

                    var fetchingParallel = videosUrlId.Select(id => youtube.FetchSafe(id, _cts.Token));

                    DownloadLabel.Content = $"Fetching Playlist Videos Data...";
                    var videosData = await Task.WhenAll(fetchingParallel);

                    _downloadQueue = videosData.Where(x => x != null).Select((x, i) => new PlaylistVideoTableRowViewModel
                    {
                        Id = YoutubeService.GetVideoUrlFromId(x.ID),
                        Index = i + 1,
                        Name = x.Title,
                        Artist = x.Artist ?? x.Creator ?? x.Channel,
                        Status = PlaylistVideoTableRowStatus.InQueue.ToString(),
                    }).ToList();

                    DownloadQueueView.Visibility = Visibility.Visible;
                    DownloadQueueView.ItemsSource = _downloadQueue;
                    DownloadLabel.Content = $"Downloading Videos{(_videoOptions.NormalizeAudio ? " and Normalizing Audio" : "")}...";
                    DownloadProgressBar.IsIndeterminate = false;

                    var processingParallel = _downloadQueue.Select(queue => RunDownloadAndNormalize(queue.Id, youtube));

                    await Task.WhenAll(processingParallel);

                    DownloadFinished("Playlist Download Finished.");

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
            catch (AggregateException ex)
            {
                var message = ex.Message;
                var innerException = ex.InnerException;

                if (innerException != null) 
                {
                    message = $"{innerException.Message}\n{ex.Message}";
                }

                CloseOnException(message);
            }
            catch (Exception ex)
            {
                CloseOnException(ex.Message);
            }
        }

        private void CloseOnException(IEnumerable<string> exceptionMessages)
        {
            CloseOnException(string.Join("\n", exceptionMessages));
        }

        private void CloseOnException(string exceptionMessage)
        {
            MessageBox.Show(exceptionMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            Close();
        }

        private void DownloadFinished(string content = "Download Finished.")
        {
            CancelBtn.IsEnabled = false;
            OkBtn.IsEnabled = true;
            OkBtn.IsDefault = true;
            DownloadLabel.Content = content;
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
            Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            _cts.Cancel();

            Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _cts.Cancel();
        }
    }
}
