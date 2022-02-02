using System;

namespace Settings
{
    public class SongSuggestSettings
    {
        public String scoreSaberID { get; set; }
        public int rankFrom { get; set; } = 1;
        public int rankTo { get; set; } = 10000;
        public bool ignorePlayedAll { get; set; } = false;
        public int ignorePlayedDays { get; set; } = 60;
        public int styleFocus { get; set; } = 0;
        public bool useLikedSongs { get; set; } = false;
        public bool fillLikedSongs { get; set; } = true;
        public PlaylistSettings playlistSettings { get; set; }
    }
}
