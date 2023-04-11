using Felipe.YoutubeExtractor.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Felipe.YoutubeExtractor.ViewModels
{
    public class PlaylistVideoTableRowViewModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Artist { get; set; }

        private PlaylistVideoTableRowStatus _status;
        public string Status 
        { 
            get => _status.ToString().CapitalizedToSpaceJoined(); 
            set  
            {
                var valid = Enum.TryParse<PlaylistVideoTableRowStatus>(value, out var valueEnum);
                if (!valid)
                {
                    throw new InvalidOperationException($"Value \"{value}\" not a valid option");
                };
                _status = valueEnum;
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
