using System;

namespace LinkedData
{
    public class Top10kSongMeta
    {
        public String songID { get; set; }
        public double count { get; set; } = 0;
        public double totalRank { get; set; } = 0;
        public double maxScore { get; set; } = 0;
        public double minScore { get; set; } = double.MaxValue;

        //Used for localvsglobal PP
        //---
        public double totalScore { get; set; } = 0;
        public double averageScore { get; set; } = 0;
        //---
    }
}