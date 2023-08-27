using System;

namespace AccSaberJson
{
    public class AccSaberSongMeta
    {
        public string songName { get; set; }
        public string songSubName { get; set; }
        public string songAuthorName { get; set; }
        public string levelAuthorName { get; set; }
        public string difficulty { get; set; }
        public string leaderboardId { get; set; }
        public string beatSaverKey { get; set; }
        public string songHash { get; set; }
        public double complexity { get; set; }
        public string categoryDisplayName { get; set; }
        public DateTime dateRanked { get; set; }
    }
}