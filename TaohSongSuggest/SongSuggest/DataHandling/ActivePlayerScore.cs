using System;

namespace ActivePlayerData
{
    public class ActivePlayerScore
    {
        public String songID { get; set; }
        public DateTime timeSet { get; set; }
        public float pp { get; set; }
        public double accuracy { get; set; }
        public double rankPercentile { get; set; }
        public int rankScoreSaber { get; set; }
        public int playsScoreSaber { get; set; }
    }
}
