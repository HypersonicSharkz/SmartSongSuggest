using System;
using System.Windows.Forms; //Application
using System.IO; //Path

namespace Settings
{
    public class FilePathSettings
    {
        public String songLibraryPath { get; set; } = Path.GetDirectoryName(Application.ExecutablePath) + "\\";
        public String playlistPath { get; set; } = Path.GetDirectoryName(Application.ExecutablePath) + "\\";
        public String activePlayerDataPath { get; set; } = Path.GetDirectoryName(Application.ExecutablePath) + "\\Players\\";
        public String top10kPlayersPath { get; set; } = Path.GetDirectoryName(Application.ExecutablePath) + "\\";
        public String bannedSongsPath { get; set; } = Path.GetDirectoryName(Application.ExecutablePath) + "\\";
        public String likedSongsPath { get; set; } = Path.GetDirectoryName(Application.ExecutablePath) + "\\";
        public String filesDataPath { get; set; } = Path.GetDirectoryName(Application.ExecutablePath) + "\\";
        public String lastSuggestionsPath { get; set; } = Path.GetDirectoryName(Application.ExecutablePath) + "\\";
    }
}

