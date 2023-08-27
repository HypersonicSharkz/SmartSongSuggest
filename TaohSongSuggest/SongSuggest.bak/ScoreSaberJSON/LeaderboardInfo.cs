using System;

namespace ScoreSabersJson
{
    public class LeaderboardInfo
    {
        public long id { get; set; }
        public string songHash { get; set; }
        public string songName { get; set; }
        public string songSubName { get; set; }
        public string songAuthorName { get; set; }
        public string levelAuthorName { get; set; }
        public Difficulty difficulty { get; set; }
        public int maxScore { get; set; }
        public DateTime createdDate { get; set; }
        public DateTime rankedDate { get; set; }
        public DateTime qualifiedDate { get; set; }
        public DateTime lovedDate { get; set; }
        public bool ranked { get; set; }
        public bool qualified { get; set; }
        public bool loved { get; set; }
        public float maxPP { get; set; }
        public float stars { get; set; }
        public int plays { get; set; }
        public int dailyPlays { get; set; }
        public bool positiveModifiers { get; set; }
        public string coverImage { get; set; }
        public Score playerScore { get; set; }
        public Difficulty[] difficulties { get; set; }
    }

}
