using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeDLSharp.Options;

namespace Felipe.YoutubeDownloader
{
    public class VideoOptionsModel : OptionsModel
    {
        public string YoutubeUrl { get; set; }
        
        public bool IsPlaylist { get; set; }
    }
}
