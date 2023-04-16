﻿namespace Felipe.YoutubeExtractor
{
    public class VideoOptionsModel : OptionsModel
    {
        public VideoOptionsModel(OptionsModel config)
        {
            OutputPath = config.OutputPath;
            OutputPathDefaultFolder = config.OutputPathDefaultFolder;

            YtdlpPath = config.YtdlpPath;
            YtdlpPathDefaultFolder = config.YtdlpPathDefaultFolder;

            FfmpegPath = config.FfmpegPath;
            FfmpegPathDefaultFolder = config.FfmpegPathDefaultFolder;

            EnableNotifications = config.EnableNotifications;
        }

        public string YoutubeUrl { get; set; }
        
        public bool IsPlaylist { get; set; }
    }
}
