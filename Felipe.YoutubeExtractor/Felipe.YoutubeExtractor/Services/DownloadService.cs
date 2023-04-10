using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Felipe.YoutubeExtractor.Extensions;
using System.Security.Policy;

namespace Felipe.YoutubeExtractor.Services
{
    class DownloadService
    {
        private const string BASE_YTDLP_URL = "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp";
        private const string FFMPEG_API_URL = "https://ffbinaries.com/api/v1/version/latest";

        /// <summary>
        /// Downloads the YT-DLP binary
        /// </summary>
        /// <param name="directoryPath">The optional directory of where it should be saved to</param>
        /// <exception cref="Exception"></exception>
        public static async Task DownloadYtDlp(CancellationToken cancellationToken, string directoryPath = "", IProgress<float> progress = null)
        {
            var downloadUrl = $"{BASE_YTDLP_URL}.exe";

            await DownloadFileBytesAsync(downloadUrl, cancellationToken, progress);
        }

        /// <summary>
        /// Downloads the FFmpeg binary
        /// </summary>
        /// <param name="directoryPath">The optional directory of where it should be saved to</param>
        /// <exception cref="Exception"></exception>
        public static async Task DownloadFFmpeg(CancellationToken cancellationToken,  IProgress<float> progress = null)
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(FFMPEG_API_URL);
            var jsonData = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<JToken>(jsonData);
            
            string ffmpegURL = result["bin"]["windows-64"]["ffmpeg"].ToString();
            await DownloadFileBytesAsync(ffmpegURL, cancellationToken, progress);

            var filename = Path.GetFileName(ffmpegURL);
            var path = Path.Combine(Directory.GetCurrentDirectory(), filename);

            using var stream = File.Open(path, FileMode.Open, FileAccess.ReadWrite);
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read, true);

            if (!archive.Entries.Any()) 
            {
                return;
            }

            var entry = archive.Entries.FirstOrDefault();

            if (entry == null) 
            { 
                return;
            }

            var resultPath = Path.Combine(Directory.GetCurrentDirectory(), entry.FullName);

            entry.ExtractToFile(resultPath, true);

            stream.Dispose();
            archive.Dispose();

            File.Delete(path);
        }

        /// <summary>
        /// Downloads a file from the specified URI
        /// </summary>
        /// <param name="uri"></param>
        /// <returns>Returns a byte array of the file that was downloaded</returns>
        /// <exception cref="InvalidOperationException"></exception>
        private static async Task DownloadFileBytesAsync(string uri, CancellationToken cancellationToken, IProgress<float> progress = null)
        {
            if (!Uri.TryCreate(uri, UriKind.Absolute, out var uriResult))
                throw new InvalidOperationException("URI is invalid.");

            using var httpClient = new HttpClient();
            var filename = Path.GetFileName(uri);
            var path = Path.Combine(Directory.GetCurrentDirectory(), filename);
            using var file = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            using var stream = new StreamReader(file);
            await httpClient.DownloadAsync(uri, stream.BaseStream, progress, cancellationToken);
            //var fileBytes = await httpClient.GetByteArrayAsync(uri, cancellationToken);
            //return fileBytes;
        }
    }
}
