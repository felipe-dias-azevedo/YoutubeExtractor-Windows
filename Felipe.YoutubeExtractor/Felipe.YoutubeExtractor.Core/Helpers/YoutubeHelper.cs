using System.Text.RegularExpressions;

namespace Felipe.YoutubeExtractor.Core.Helpers;

public static class YoutubeHelper
{
    private const string _validYoutubeUrl = @"^(https?\:\/\/)?((www\.)?youtube\.com|youtu\.be)\/.+$";
    
    public static bool IsValidUrl(string url)
    {
        return Regex.IsMatch(url, _validYoutubeUrl);
    }
}