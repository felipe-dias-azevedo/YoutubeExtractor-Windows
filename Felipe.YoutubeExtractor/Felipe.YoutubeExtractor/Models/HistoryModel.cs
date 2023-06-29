using Felipe.YoutubeExtractor.Core.Extensions;
using Felipe.YoutubeExtractor.Extensions;
using System;

namespace Felipe.YoutubeExtractor.Models
{
    public class HistoryModel
    {
        public long Id { get; set; }
        public string YoutubeId { get; set; }
        public string? Title { get; set; }
        public string? Artist { get; set; }
        public HistoryType HistoryTypeEnum { get; private set; }
        public string HistoryType 
        {
            get => HistoryTypeEnum.ToString().CapitalizedToSpaceJoined();
            set
            {
                var valueString = value.RemoveAllWhitespaces();

                var valid = Enum.TryParse<HistoryType>(valueString, out var valueEnum);
                if (!valid)
                {
                    throw new InvalidOperationException($"Value \"{value}\" not a valid option");
                };
                HistoryTypeEnum = valueEnum;
            }
        }
        public DateTime DateTime { get; set; }
    }

    public enum HistoryType
    {
        SingleVideo,
        Playlist
    }
}
