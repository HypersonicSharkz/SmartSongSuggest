using SongSuggestNS;
using System;

namespace Settings
{
    public class OldAndNewSettings
    {
        public String scoreSaberID { get; set; }

        public double ignoreAccuracyEqualAbove { get; set; } = 100.0;
        public double ignoreAccuracyEqualBelow { get; set; } = 0;

        
        public double ignoreBeatSaberStarAbove { get; set; } = 20;
        public double ignoreBeatSaberStarBelow { get; set; } = 0;

        
        public double ignoreAccSaberComplexityAbove { get; set; } = 20;
        public double ignoreAccSaberComplexityBelow { get; set; } = 0;
        

        public int ignorePlayedDaysAbove { get; set; } = int.MaxValue;
        public int ignorePlayedDaysBelow { get; set; } = 0;

        public PlaylistSettings playlistSettings {get;set;}

        public SongCategory playedSongCategories { get; set; } = SongCategory.ScoreSaber;
        public SongCategory unplayedSongCategories { get; set; } = 0;

        public int playlistLength { get; set; } = 100;

        public SongSortCriteria songSelection { get; set; } = SongSortCriteria.Age;
        public bool reverseSelectionOrdering { get; set; } = true;

        //Perform Weighted Sorting (0 = Standard, 1 = Random, rest is weighted search applied at various strength on list order.
        //values that is N/A or 0 is treated as average weighted (e.g. PP on unplayed would be given same weight as a median played)).
        public double songWeighting { get; set; }

        public SongSortCriteria songOrdering { get; set; } = SongSortCriteria.Age;
        public bool reversePlaylistOrdering { get; set; } = false;
        //Takes priority over reverse playlist ordering
        public bool randomizePlaylistOrdering { get; set; } = false;
    }
}
