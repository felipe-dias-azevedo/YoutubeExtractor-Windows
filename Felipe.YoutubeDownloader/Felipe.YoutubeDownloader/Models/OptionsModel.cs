using Felipe.YoutubeDownloader.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeDLSharp.Options;

namespace Felipe.YoutubeDownloader
{
    public class OptionsModel
    {
        private string _ytdlpPath;
        public string YtdlpPath 
        {
            get => YtdlpPathDefaultFolder
                ? GetYtDlpDefaultFolder()
                : _ytdlpPath;
            set { _ytdlpPath = value; }
        }
        public bool YtdlpPathDefaultFolder { get; set; } = true;

        private string _ffmpegPath;
        public string FfmpegPath 
        { 
            get => FfmpegPathDefaultFolder 
                ? GetFfmpegDefaultFolder()
                : _ffmpegPath;
            set => _ffmpegPath = value;
        }
        public bool FfmpegPathDefaultFolder { get; set; } = true;

        private string _outputPath;
        public string OutputPath 
        { 
            get => OutputPathDefaultFolder 
                ? GetOutputDefaultFolder()
                : _outputPath;
            set => _outputPath = value; 
        }
        public bool OutputPathDefaultFolder { get; set; } = true;

        public DownloadTypeFormat DownloadType { get; set; } = DownloadTypeFormat.OnlyAudio;
        public string Format { get; set; } = "bestaudio";
        public bool BestAudio { get; set; } = true;
        public AudioConversionFormat AudioFormat { get; set; } = AudioConversionFormat.Mp3;
        public bool Metadata { get; set; } = true;
        public bool EmbedThumbnail { get; set; }

        public static string GetYtDlpDefaultFolder()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "yt-dlp.exe");
        }

        public static string GetFfmpegDefaultFolder()
        {
            return Directory.GetCurrentDirectory();
        }

        public static string GetFfmpegDefaultFile()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "ffmpeg.exe");
        }

        public static string GetOutputDefaultFolder()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "output");
        }
    }
}
