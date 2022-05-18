using System;

namespace Settings
{
    //All these values MUST be set before passing it on to SongSuggest, suggested default locations in comments.
    public class FilePathSettings
    {
        public String songLibraryPath { get; set; }// "\\";
        public String playlistPath { get; set; }// "\\";
        public String activePlayerDataPath { get; set; }// "\\Players\\";
        public String top10kPlayersPath { get; set; }// "\\";
        public String bannedSongsPath { get; set; }// "\\";
        public String likedSongsPath { get; set; }// "\\";
        public String filesDataPath { get; set; }// "\\";
        public String lastSuggestionsPath { get; set; }// "\\";
    }
}

