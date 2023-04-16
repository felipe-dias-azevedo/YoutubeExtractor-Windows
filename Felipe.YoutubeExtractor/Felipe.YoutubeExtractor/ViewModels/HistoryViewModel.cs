using Felipe.YoutubeExtractor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Felipe.YoutubeExtractor.ViewModels
{
    public class HistoryViewModel
    {
        public long Id { get; set; }
        public string HistoryType { get; set; }
        public string YoutubeId { get; set; }
        public string? Title { get; set; }
        public string? Artist { get; set; }
        public DateTime DateTime { get; set; }
    }
}
