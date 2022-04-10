using System;

namespace ScoreSabersJson
{
    public class Score
    {
        public long id { get; set; }
        public LeaderboardPlayer leaderboardPlayerInfo { get; set; }
        public int rank { get; set; }
        public int baseScore { get; set; }
        public int modifiedScore { get; set; }
        public float pp { get; set; }
        public float weight { get; set; }
        public string modifiers { get; set; }
        public float multiplier { get; set; }
        public int badCuts { get; set; }
        public int missedNotes { get; set; }
        public int maxCombo { get; set; }
        public bool fullCombo { get; set; }
        public int hmd { get; set; }
        public DateTime timeSet { get; set; }
        public bool hasReplay { get; set; }
    }
}
