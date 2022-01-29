using System;

namespace Settings
{
    public class OldestSongSettings
    {
        public String scoreSaberID { get; set; }
        public double ignoreAccuracyEqualAbove { get; set; } = 100.0;
        public int ignorePlayedDays { get; set; } = 0;
    }
}
