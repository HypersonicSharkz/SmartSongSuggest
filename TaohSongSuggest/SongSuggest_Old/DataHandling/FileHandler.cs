using System;
using System.Windows.Forms;
using SongLibraryNS;
using System.IO;
using ActivePlayerData;
using Newtonsoft.Json;
using LinkedData;
using BanLike;
using System.Collections.Generic;
using Data;
using DataHandling;
using Settings;


namespace FileHandling
{
    public class FileHandler
    {
        private JsonSerializerSettings serializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

        public ToolBox toolBox { get; set; }
 
        public String songLibraryPath { get; set; } = Path.GetDirectoryName(Application.ExecutablePath) + "\\";
        public String playlistPath { get; set; } = Path.GetDirectoryName(Application.ExecutablePath) + "\\";
        public String activePlayerDataPath { get; set; } = Path.GetDirectoryName(Application.ExecutablePath) + "\\Players\\";
        public String top10kPlayersPath { get; set; } = Path.GetDirectoryName(Application.ExecutablePath) + "\\";
        public String bannedSongsPath { get; set; } = Path.GetDirectoryName(Application.ExecutablePath) + "\\";
        public String likedSongsPath { get; set; } = Path.GetDirectoryName(Application.ExecutablePath) + "\\";
        public String filesDataPath { get; set; } = Path.GetDirectoryName(Application.ExecutablePath) + "\\";

        public void UpdatePaths(FilePathSettings filePathSettings)
        {
            if (filePathSettings.songLibraryPath != null) this.songLibraryPath = filePathSettings.songLibraryPath;
            if (filePathSettings.playlistPath != null) this.playlistPath = filePathSettings.playlistPath;
            if (filePathSettings.activePlayerDataPath != null) this.activePlayerDataPath = filePathSettings.activePlayerDataPath;
            if (filePathSettings.top10kPlayersPath != null) this.top10kPlayersPath = filePathSettings.top10kPlayersPath;
            if (filePathSettings.bannedSongsPath != null) this.bannedSongsPath = filePathSettings.bannedSongsPath;
            if (filePathSettings.likedSongsPath != null) this.likedSongsPath = filePathSettings.likedSongsPath;
            if (filePathSettings.filesDataPath != null) this.filesDataPath = filePathSettings.filesDataPath;
        }

        //Loads the primary song library from disc
        //Needs to be changed so JSON is handled here instead of in library
        public void LoadSongLibrary(SongLibrary songLibrary)
        {
            if (!File.Exists(songLibraryPath + "SongLibrary.json")) SaveSongLibrary("[]");
            String songLibraryJSON = File.ReadAllText(songLibraryPath+"SongLibrary.json");
            songLibrary.SetJSON(songLibraryJSON);
        }

        //Needs updating so JSON is handled here instead of in library
        public void SaveSongLibrary(String songLibraryJSON)
        {
            File.WriteAllText(songLibraryPath+ "SongLibrary.json", songLibraryJSON);
        }

        //Add the songs from a library from the disc to the to the active library, and if new songs was added save the active library.
        //Needs JSON handling moved here instead of library.
        public void AddSongLibrary(String path)
        {
            String songLibraryJSON = File.ReadAllText(path);
            toolBox.songLibrary.AddLibrary(songLibraryJSON);
            if (toolBox.songLibrary.Updated()) toolBox.songLibrary.Save();
        }

        public void SavePlaylist(String playlistString, String fileName)
        {
            File.WriteAllText(playlistPath + fileName + ".bplist", playlistString);
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
            String linkPlayerJSON = File.ReadAllText(top10kPlayersPath+ "Top10KPlayers.json");
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
            if (!File.Exists(likedSongsPath+"Liked Songs.json")) SaveLikedSongs(new List<SongLike>());
            String likedSongsString = File.ReadAllText(likedSongsPath + "Liked Songs.json");
            return JsonConvert.DeserializeObject<List<SongLike>>(likedSongsString, serializerSettings);
        }

        public void SaveLikedSongs(List<SongLike> songLiking)
        {
            File.WriteAllText(likedSongsPath + "Liked Songs.json", JsonConvert.SerializeObject(songLiking));
        }

        public List<SongBan> LoadBannedSongs()
        {
            if (!File.Exists(bannedSongsPath+"Banned Songs.json")) SaveBannedSongs(new List<SongBan>());
            String bannedSongsString = File.ReadAllText(bannedSongsPath + "Banned Songs.json");
            return JsonConvert.DeserializeObject<List<SongBan>>(bannedSongsString, serializerSettings);            
        }

        public void SaveBannedSongs(List<SongBan> songBans)
        {
            File.WriteAllText(bannedSongsPath + "Banned Songs.json", JsonConvert.SerializeObject(songBans));
        }

        public FilesData LoadFilesData()
        {
            if (!File.Exists(filesDataPath + "Files.meta")) SaveFilesData(new FilesData());
            String filesDataString = File.ReadAllText(filesDataPath + "Files.meta");
            return JsonConvert.DeserializeObject<FilesData>(filesDataString, serializerSettings);
        }

        public void SaveFilesData(FilesData filesData)
        {
            File.WriteAllText(filesDataPath + "Files.meta", JsonConvert.SerializeObject(filesData));
        }

    }

}