using System;

namespace Settings
{
    public class SongSuggestSettings
    {
        public String scoreSaberID { get; set; }
        public int rankFrom { get; set; }
        public int rankTo { get; set; }
        public bool ignorePlayedAll { get; set; }
        public int ignorePlayedDays { get; set; }
        public int styleFocus { get; set; }
        public bool useLikedSongs { get; set; }
        public bool fillLikedSongs { get; set; }
    }
}
