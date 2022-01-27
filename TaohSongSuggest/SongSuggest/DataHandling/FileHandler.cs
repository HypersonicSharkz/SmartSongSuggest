using System;
using System.Windows.Forms;
using SongLibraryNS;
using System.IO;
using ActivePlayerData;
using Newtonsoft.Json;
using LinkedData;
using BanLike;
using System.Collections.Generic;
using System.Reflection;
using IPA.Utilities;

namespace FileHandling
{
    public class FileHandler
    {
        private JsonSerializerSettings serializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

        public static readonly string configDir = Path.Combine(UnityGame.UserDataPath, "TaohSongSuggest") + "/";

        public static String songLibraryPath = configDir;
        public static String playlistPath = Path.Combine(UnityGame.InstallPath, "Playlists/");
        public static String activePlayerDataPath = Path.Combine(configDir, "Players/");
        public static String top10kPlayersPath = "TaohSongSuggest.Configuration.InitialData.Top10KPlayers.json";
        public static String bannedSongsPath = configDir;
        public static String likedSongsPath = configDir;


        public void LoadSongLibrary(SongLibrary songLibrary)
        {
            if (!File.Exists(songLibraryPath + "SongLibrary.json")) SaveSongLibrary("[]");

            String songLibraryJSON = File.ReadAllText(songLibraryPath+"SongLibrary.json");

            songLibrary.SetJSON(songLibraryJSON);
        }

        public void SaveSongLibrary(String songLibraryJSON)
        {
            File.WriteAllText(songLibraryPath+ "SongLibrary.json", songLibraryJSON);
        }

        public void SavePlaylist(String playlistString, String fileName)
        {
            File.WriteAllText(playlistPath + fileName + ".bplist", playlistString);
        }

        public void RemovePlaylist(String fileName)
        {
            File.Delete(playlistPath + fileName + ".bplist");
        }

        public Boolean ActivePlayerExist(String scoreSaberID)
        {
            return File.Exists(activePlayerDataPath + scoreSaberID + ".json");
        }

        public ActivePlayer LoadActivePlayer(String scoreSaberID)
        {
            String activePlayerString = File.ReadAllText(activePlayerDataPath + scoreSaberID + ".json");
            return JsonConvert.DeserializeObject<ActivePlayer>(activePlayerString, serializerSettings);
        }

        public void SaveActivePlayer(ActivePlayer activePlayer)
        {
            File.WriteAllText(activePlayerDataPath + activePlayer.id + ".json", JsonConvert.SerializeObject(activePlayer));
        }

        public Top10kPlayers LoadLinkedData()
        {
            string linkPlayerJSON;
            var assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream(top10kPlayersPath))
            using (StreamReader reader = new StreamReader(stream))
            {
                linkPlayerJSON = reader.ReadToEnd();
            }

            //String linkPlayerJSON = File.ReadAllText(top10kPlayersPath+ "Top10KPlayers.json");
            
            Top10kPlayers players = new Top10kPlayers();
            players.SetJSON(linkPlayerJSON);
            return players;
        }

        public void SaveLinkedData(String top10kPlayersString)
        {
            File.WriteAllText(top10kPlayersPath + "Top10kPlayers.json", top10kPlayersString);
        }
 
        public Boolean LinkedDataExist()
        {
            return File.Exists(top10kPlayersPath+ "Top10KPlayers.json");
        }

        public List<SongLike> LoadLikedSongs()
        {
            if (!File.Exists(likedSongsPath + "Liked Songs.json")) SaveLikedSongs(new List<SongLike>());

            String likedSongsString = File.ReadAllText(likedSongsPath + "Liked Songs.json");

            return JsonConvert.DeserializeObject<List<SongLike>>(likedSongsString, serializerSettings);
        }

        public void SaveLikedSongs(List<SongLike> songLiking)
        {
            File.WriteAllText(likedSongsPath + "Liked Songs.json", JsonConvert.SerializeObject(songLiking));
        }

        public List<SongBan> LoadBannedSongs()
        {
            if (!File.Exists(bannedSongsPath + "Banned Songs.json")) SaveBannedSongs(new List<SongBan>());

            String bannedSongsString = File.ReadAllText(bannedSongsPath + "Banned Songs.json");

            return JsonConvert.DeserializeObject<List<SongBan>>(bannedSongsString, serializerSettings);
        }

        public void SaveBannedSongs(List<SongBan> songBans)
        {
            File.WriteAllText(bannedSongsPath + "Banned Songs.json", JsonConvert.SerializeObject(songBans));
        }

    }

}