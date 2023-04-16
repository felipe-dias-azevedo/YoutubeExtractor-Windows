using Felipe.YoutubeExtractor.Extensions;
using Felipe.YoutubeExtractor.Models;
using FFmpeg.NET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;

namespace Felipe.YoutubeExtractor.Services
{
    public class YoutubeService
    {
        private readonly VideoOptionsModel _videoOptions;
        private readonly YoutubeDL _youtubeDl;

        private const string _validYoutubeUrl = @"^(https?\:\/\/)?((www\.)?youtube\.com|youtu\.be)\/.+$";

        public YoutubeService(VideoOptionsModel videoOptions)
        {
            _videoOptions = videoOptions;
            _youtubeDl = new YoutubeDL
            {
                //YoutubeDLPath = FileService.ConvertExecutable(_videoOptions.YtdlpPath),
                YoutubeDLPath = _videoOptions.YtdlpPath,
                OutputFolder = _videoOptions.OutputPath,
                OutputFileTemplate = "%(title)s.%(ext)s"
            };
        }

        public static string GetVideoUrlFromId(string id)
        {
            return $"https://www.youtube.com/watch?v={id}";
        }

        public static string GetPlaylistUrlFromId(string id)
        {
            return $"https://www.youtube.com/playlist?list={id}";
        }

        public static bool IsValidUrl(string url)
        {
            return Regex.IsMatch(url, _validYoutubeUrl);
        }

        public async Task<string> Download(string? videoUrl = null, CancellationToken cancellationToken = default, Progress<YoutubeDLSharp.DownloadProgress>? progress = null)
        {
            var url = _videoOptions.YoutubeUrl;

            if (videoUrl != null)
            {
                url = videoUrl!;
            }

            var customOptions = new OptionSet
            {
                Format = _videoOptions.Format,
                FfmpegLocation = _videoOptions.FfmpegPath,
                AudioFormat = _videoOptions.AudioFormat,
                EmbedMetadata = _videoOptions.Metadata,
                AudioQuality = _videoOptions.BestAudio ? 0 : null,
                YesPlaylist = _videoOptions.IsPlaylist,
                EmbedThumbnail = _videoOptions.EmbedThumbnail,
                ExtractAudio = _videoOptions.DownloadType == DownloadTypeFormat.OnlyAudio,
            };

            //customOptions.AddCustomOption("--paths", _videoOptions.OutputPath);

            var res = await _youtubeDl.RunVideoDownload(url, 
                ct: cancellationToken, 
                format: _videoOptions.Format, 
                progress: progress, 
                overrideOptions: customOptions);

            if (res == null)
            {
                throw new InvalidOperationException("No response from fetching.");
            }

            if (!res.Success)
            {
                throw new Exception(string.Join("\n", res.ErrorOutput));
            }

            return res.Data;
        }

        public async Task<(List<string?>, List<string?>)> GetId(CancellationToken cancellationToken = default)
        {
            var ytdlProc = new YoutubeDLProcess(_videoOptions.YtdlpPath);

            var output = new List<string?>();
            var error = new List<string?>();

            ytdlProc.OutputReceived += (o, e) => output.Add(e.Data);
            ytdlProc.ErrorReceived += (o, e) => error.Add(e.Data);

            var urls = new[] { _videoOptions.YoutubeUrl };

            var customOptions = new OptionSet
            {
                FlatPlaylist = _videoOptions.IsPlaylist,
                Print = "id"
            };

            await ytdlProc.RunAsync(urls, customOptions, cancellationToken);

            return (output, error);
        }

        public async Task<List<string>> GetVideoIdsSafe(CancellationToken cancellationToken = default)
        {
            var (output, error) = await GetId(cancellationToken);

            return output.Where(x => !string.IsNullOrEmpty(x)).Select(x => x!).ToList();
        }

        public async Task<List<string>> GetVideoIds(CancellationToken cancellationToken = default)
        {
            var (output, error) = await GetId(cancellationToken);

            if (error.Any(x => !string.IsNullOrEmpty(x))) 
            {
                throw new Exception(string.Join("\n", error));
            }

            return output.Where(x => !string.IsNullOrEmpty(x)).Select(x => x!).ToList();
        }

        public async Task<VideoData?> FetchSafe(string? videoUrl = null, CancellationToken cancellationToken = default)
        {
            try
            {
                return await Fetch(videoUrl, cancellationToken);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<VideoData> Fetch(string? videoUrl = null, CancellationToken cancellationToken = default)
        {
            var url = _videoOptions.YoutubeUrl;

            if (videoUrl != null)
            {
                url = videoUrl;
            }

            var res = await _youtubeDl.RunVideoDataFetch(url, ct: cancellationToken/*, flat: !_videoOptions.IsPlaylist*/);
            
            if (res == null)
            {
                throw new InvalidOperationException("No response from fetching.");
            }

            if (!res.Success) 
            {
                throw new Exception(string.Join("\n", res.ErrorOutput));
            }

            return res.Data;
        }

        public async Task Normalize(string fileExportPath, CancellationToken cancellationToken = default)
        {
            var ffmpegPath = Path.Combine(_videoOptions.FfmpegPath, OptionsModel.GetFfmpegDefaultFileName());
            var ffmpeg = new Engine(ffmpegPath);

            var filePath = FileService.ConvertExecutable(fileExportPath);

            string? volume = null;

            ffmpeg.Data += (sender, e) => 
            {
                if (e.Data != null && e.Data.Contains("max_volume"))
                {
                    var tempVolume = e.Data.Split(":").LastOrDefault()?.Trim();

                    if (tempVolume == null)
                    {
                        return;
                    }

                    if (tempVolume.Contains('+'))
                    {
                        tempVolume = tempVolume.Replace("+", "-");
                    } 
                    else if (tempVolume.Contains('-'))
                    {
                        tempVolume = tempVolume.Replace("-", "");
                    }

                    volume = tempVolume.RemoveAllWhitespaces();
                }
            };

            await ffmpeg.ExecuteAsync($"-i {filePath} -filter:a volumedetect -f null NUL", cancellationToken);

            if (volume == null)
            {
                throw new InvalidOperationException();
            }

            ffmpeg.Error += (sender, e) => throw e.Exception;

            var tempOutputNames = Path.GetFileName(fileExportPath).Split(".");
            tempOutputNames[0] = tempOutputNames[0] + "-normalized";
            var tempOutputName = string.Join(".", tempOutputNames);
            var tempOutputNamePath = Path.Combine(FileService.GetFolderPathFromFilePath(fileExportPath)!, tempOutputName);
            var tempOutputPath = FileService.ConvertExecutable(tempOutputNamePath);

            await ffmpeg.ExecuteAsync($"-y -i {filePath} -movflags use_metadata_tags -map_metadata 0 -filter:a \"volume={volume}\" -q:a 0 -c:v copy {tempOutputPath}", cancellationToken);

            File.Delete(fileExportPath);
            File.Move(tempOutputNamePath, fileExportPath);
        }
    }
}
