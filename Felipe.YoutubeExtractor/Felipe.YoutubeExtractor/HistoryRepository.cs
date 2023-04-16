using Dapper;
using Felipe.YoutubeExtractor.Models;
using Felipe.YoutubeExtractor.ViewModels;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Felipe.YoutubeExtractor
{
    public class HistoryRepository
    {
        private const string _path = "history.db";
        private readonly SqliteConnection _connection;

        public HistoryRepository()
        {
            _connection = GetConnection();
        }

        private SqliteConnection GetConnection()
        {
            var connection = new SqliteConnection($"Data Source={_path}");

            connection.Open();

            connection.Execute("CREATE TABLE IF NOT EXISTS HistoryModel (" +
                "Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, " +
                "HistoryType VARCHAR(12) NOT NULL," +
                "YoutubeId VARCHAR(100) NOT NULL, " +
                "Title VARCHAR(200), " +
                "Artist VARCHAR(200)," +
                "DateTime DATETIME NOT NULL);");

            return connection;
        }

        public async Task Insert(HistoryModel historyModel)
        {
            await _connection.QueryAsync(@"INSERT INTO HistoryModel 
                (YoutubeId, HistoryType, Title, Artist, DateTime) 
                VALUES (@YoutubeId, @HistoryType, @Title, @Artist, @DateTime);",
                historyModel);
        }

        public async Task<IEnumerable<HistoryViewModel>> Get()
        {
            return await _connection.QueryAsync<HistoryViewModel>(@"SELECT Id, HistoryType, YoutubeId, Title, Artist, DateTime 
                FROM HistoryModel
                ORDER BY Id;");
        }

        public async Task<IEnumerable<HistoryViewModel>> GetByYoutubeId(string youtubeId)
        {
            return await _connection.QueryAsync<HistoryViewModel>(@"SELECT Id, HistoryType, YoutubeId, Title, Artist, DateTime 
                FROM HistoryModel 
                WHERE YoutubeId = @youtubeId
                ORDER BY Id;", 
                new { youtubeId });
        }

        public async Task Delete()
        {
            await _connection.QueryAsync(@"DELETE FROM HistoryModel;");
        }
    }
}
