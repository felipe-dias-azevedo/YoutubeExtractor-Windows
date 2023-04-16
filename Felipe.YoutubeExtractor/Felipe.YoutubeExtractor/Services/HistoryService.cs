using Felipe.YoutubeExtractor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeDLSharp;

namespace Felipe.YoutubeExtractor.Services
{
    public class HistoryService
    {
        private readonly HistoryRepository _repository;

        public HistoryService()
        {
            _repository = new HistoryRepository();
        }

        public async Task Insert(string youtubeId, HistoryType historyType, string? title = null, string? artist = null)
        {
            var history = new HistoryModel 
            { 
                YoutubeId = youtubeId, 
                Title = title, 
                Artist = artist, 
                HistoryType = historyType.ToString(),
                DateTime = DateTime.Now 
            };

            await _repository.Insert(history);
        }

        public async Task<IList<HistoryModel>> Get()
        {
            var history = await _repository.Get();

            return history.Select(x => new HistoryModel
            {
                Id = x.Id,
                Title = x.Title,
                DateTime = x.DateTime,
                HistoryType = x.HistoryType,
                Artist = x.Artist,
                YoutubeId = x.YoutubeId
            }).ToList();
        }

        public async Task<IList<HistoryModel>> GetByYoutubeId(string youtubeId)
        {
            var history = await _repository.GetByYoutubeId(youtubeId);

            return history.Select(x => new HistoryModel
            {
                Id = x.Id,
                Title = x.Title,
                DateTime = x.DateTime,
                HistoryType = x.HistoryType,
                Artist = x.Artist,
                YoutubeId = x.YoutubeId
            }).ToList();
        }

        public async Task Delete()
        {
            await _repository.Delete();
        }
    }
}
