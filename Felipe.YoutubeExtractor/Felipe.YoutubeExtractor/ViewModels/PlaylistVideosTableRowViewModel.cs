using Felipe.YoutubeExtractor.Core.Extensions;
using Felipe.YoutubeExtractor.Extensions;
using System;

namespace Felipe.YoutubeExtractor.ViewModels
{
    public class PlaylistVideoTableRowViewModel
    {
        public string Id { get; set; }

        public int Index { get; set; }

        public string Name { get; set; }

        public string Artist { get; set; }

        public PlaylistVideoTableRowStatus StatusEnum { get; private set; }
        public string Status 
        { 
            get => StatusEnum.ToString().CapitalizedToSpaceJoined(); 
            set  
            {
                var valid = Enum.TryParse<PlaylistVideoTableRowStatus>(value, out var valueEnum);
                if (!valid)
                {
                    throw new InvalidOperationException($"Value \"{value}\" not a valid option");
                };
                StatusEnum = valueEnum;
            }
        }
    }

    public enum PlaylistVideoTableRowStatus
    {
        InQueue,
        DownloadInProgress,
        DownloadDone,
        NormalizeInProgress,
        Done,
        Error
    }
}
