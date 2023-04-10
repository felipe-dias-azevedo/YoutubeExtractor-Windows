using Felipe.YoutubeExtractor.Extensions;
using Felipe.YoutubeExtractor.Models;
using FFmpeg.NET;
using FFmpeg.NET.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using YoutubeDLSharp;
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
                YoutubeDLPath = FileService.ConvertExecutable(_videoOptions.YtdlpPath),
                OutputFolder = _videoOptions.OutputPath
            };
        }

        public static bool IsValidUrl(string url)
        {
            return Regex.IsMatch(url, _validYoutubeUrl);
        }

        public async Task<RunResult<string>> Download(CancellationToken cancellationToken = default, Progress<YoutubeDLSharp.DownloadProgress>? progress = null)
        {
            var customOptions = new OptionSet
            {
                Format = _videoOptions.Format,
                FfmpegLocation = _videoOptions.FfmpegPath,
                AudioFormat = _videoOptions.AudioFormat,
                AddMetadata = _videoOptions.Metadata,
                AudioQuality = _videoOptions.BestAudio ? 0 : null,
                YesPlaylist = _videoOptions.IsPlaylist,
                EmbedThumbnail = _videoOptions.EmbedThumbnail,
                ExtractAudio = _videoOptions.DownloadType == DownloadTypeFormat.OnlyAudio,
            };

            //customOptions.AddCustomOption("--paths", _videoOptions.OutputPath);

            return await _youtubeDl.RunVideoDownload(_videoOptions.YoutubeUrl, 
                ct: cancellationToken, 
                format: _videoOptions.Format, 
                progress: progress, 
                overrideOptions: customOptions);
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

            await ffmpeg.ExecuteAsync($"-y -i {filePath} -movflags use_metadata_tags -map_metadata 0 -filter:a \"volume={volume}\" -q:a 0 {tempOutputPath}", cancellationToken);

            File.Delete(fileExportPath);
            File.Move(tempOutputNamePath, fileExportPath);
        }
    }
}
