using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BS_Utils;

namespace TaohSongSuggest.Utils
{
    static class ConfigUtil
    {
        public static readonly string configDir = Path.Combine(UnityGame.UserDataPath, "TaohSongSuggest");

        public static string songLibraryPath = Path.Combine(configDir, "SongLibrary.json");
        public static string playListPath = Path.Combine(UnityGame.InstallPath, "Playlists/SongSuggestPlaylist.bplist");
        public static string activePlayerDataPath = Path.Combine(configDir, "PlayerData.json");
        public static string top10kPlayersPath = "TaohSongSuggest.Configuration.InitialData.Top10KPlayers.json";
    }
}
