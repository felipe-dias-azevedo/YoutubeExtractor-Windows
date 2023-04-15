namespace Felipe.YoutubeExtractor
{
    public class VideoOptionsModel : OptionsModel
    {
        public string YoutubeUrl { get; set; }
        
        public bool IsPlaylist { get; set; }
    }
}
