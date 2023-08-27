using SongSuggestNS;
using System;

namespace Settings
{
    public class OldestSongSettings
    {
        public String scoreSaberID { get; set; }
        public double ignoreAccuracyEqualAbove { get; set; } = 100.0;
        public double ignoreAccuracyEqualBellow { get; set; } = 0;

        public double beatSaberStarEqualMinIncluded { get; set; } = 0;
        public double beatSaberStarMaxIncluded { get; set; } = 20;
        public int ignorePlayedDays { get; set; } = 0;
        public PlaylistSettings playlistSettings {get;set;}
        public SongCategory playedSongCategories { get; set; } = SongCategory.ScoreSaber;
        public SongCategory unplayedSongCategories { get; set; } = 0;
        public int playlistLength { get; set; } = 100;
        public SongSortCriteria songSelection { get; set; } = SongSortCriteria.Age;
        public SongSortCriteria songOrdering { get; set; } = SongSortCriteria.Age;
        public bool reverseSelectionOrdering { get; set; } = true;
        public bool reversePlaylistOrdering { get; set; } = false;
    }
}
