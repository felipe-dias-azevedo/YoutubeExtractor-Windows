using Felipe.YoutubeExtractor.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    }
}
