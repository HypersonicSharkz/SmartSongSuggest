using System;
using SongLibraryNS;
using System.IO;
using ActivePlayerData;
using Newtonsoft.Json;
using LinkedData;
using BanLike;
using System.Collections.Generic;
using Data;
using SongSuggestNS;
using Settings;
using PlaylistJson;


namespace FileHandling
{
    public class FileHandler
    {
        private JsonSerializerSettings serializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

        public SongSuggest songSuggest { get; set; }
        public FilePathSettings filePathSettings { get; set; }

        //Loads the primary song library from disc
        public List<Song> LoadSongLibrary()
        {
            if (!File.Exists(filePathSettings.songLibraryPath + "SongLibrary.json")) SaveSongLibrary(new List<Song>());
            String songLibraryJSON = File.ReadAllText(filePathSettings.songLibraryPath + "SongLibrary.json");
            return JsonConvert.DeserializeObject<List<Song>>(songLibraryJSON, serializerSettings);
        }

        //Save the known Songs in the Library
        public void SaveSongLibrary(List<Song> songLibrary)
        {
            File.WriteAllText(filePathSettings.songLibraryPath + "SongLibrary.json", JsonConvert.SerializeObject(songLibrary));
        }

        ////Add the songs from a library from the disc to the to the active library, and if new songs was added save the active library.
        //public void AddSongLibrary(String path)
        //{
        //    String songLibraryJSON = File.ReadAllText(path);
        //    songSuggest.songLibrary.AddLibrary(JsonConvert.DeserializeObject<List<Song>>(songLibraryJSON, serializerSettings));
        //}

        public void SavePlaylist(String playlistString, String fileName)
        {
            File.WriteAllText(filePathSettings.playlistPath + fileName + ".bplist", playlistString);
        }

        //Load Playlist json
        public Playlist LoadPlaylist(String fileName)
        {
            if (!File.Exists(filePathSettings.playlistPath + fileName + ".bplist")) SavePlaylist(new Playlist(), fileName);
            String playlistString = File.ReadAllText(filePathSettings.playlistPath + fileName + ".bplist");
            return JsonConvert.DeserializeObject<Playlist>(playlistString, serializerSettings);
        }

        //save Playlist json
        public void SavePlaylist(Playlist playlist, String fileName)
        {
            File.WriteAllText(filePathSettings.playlistPath + fileName + ".bplist", JsonConvert.SerializeObject(playlist, serializerSettings));
        }



        //Load Active Players Data
        public ActivePlayer LoadActivePlayer(String scoreSaberID)
        {
            if (!File.Exists(filePathSettings.activePlayerDataPath + scoreSaberID+ ".json")) SaveActivePlayer(new ActivePlayer(), scoreSaberID);
            String activePlayerString = File.ReadAllText(filePathSettings.activePlayerDataPath + scoreSaberID + ".json");
            return JsonConvert.DeserializeObject<ActivePlayer>(activePlayerString, serializerSettings);
        }

        //Save Active Players Data
        public void SaveActivePlayer(ActivePlayer activePlayer, String fileName)
        {
            File.WriteAllText(filePathSettings.activePlayerDataPath + fileName+ ".json", JsonConvert.SerializeObject(activePlayer));
        }

        //Is there a player Refresh File
        public Boolean CheckPlayerRefresh()
        {
            return File.Exists(filePathSettings.activePlayerDataPath + "RefreshPlayer.txt");
        }

        //Adds/removes the player refresh file
        public void TogglePlayerRefresh()
        {
            if(CheckPlayerRefresh())
            //Remove File
            {
                File.Delete(filePathSettings.activePlayerDataPath + "RefreshPlayer.txt");
            }
            //Add File
            else
            {
                File.WriteAllText(filePathSettings.activePlayerDataPath + "RefreshPlayer.txt", "Remind Me: Reset Player Profile");
            }
        }

        public List<Top10kPlayer> LoadLinkedData()
        {
            if (!File.Exists(filePathSettings.likedSongsPath + "Top10KPlayers.json")) SaveLinkedData(new List<Top10kPlayer>());
            String linkPlayerJSON = File.ReadAllText(filePathSettings.top10kPlayersPath + "Top10KPlayers.json");
            return JsonConvert.DeserializeObject<List<Top10kPlayer>>(linkPlayerJSON, serializerSettings);
        }

        public void SaveLinkedData(List<Top10kPlayer> players)
        {
            File.WriteAllText(filePathSettings.top10kPlayersPath + "Top10KPlayers.json", JsonConvert.SerializeObject(players));
        }
 
        public Boolean LinkedDataExist()
        {
            return File.Exists(filePathSettings.top10kPlayersPath + "Top10KPlayers.json");
        }

        public List<SongLike> LoadLikedSongs()
        {
            if (!File.Exists(filePathSettings.likedSongsPath +"Liked Songs.json")) SaveLikedSongs(new List<SongLike>());
            String likedSongsString = File.ReadAllText(filePathSettings.likedSongsPath + "Liked Songs.json");
            return JsonConvert.DeserializeObject<List<SongLike>>(likedSongsString, serializerSettings);
        }

        public void SaveLikedSongs(List<SongLike> songLiking)
        {
            File.WriteAllText(filePathSettings.likedSongsPath + "Liked Songs.json", JsonConvert.SerializeObject(songLiking));
        }

        public List<SongBan> LoadBannedSongs()
        {
            if (!File.Exists(filePathSettings.bannedSongsPath +"Banned Songs.json")) SaveBannedSongs(new List<SongBan>());
            String bannedSongsString = File.ReadAllText(filePathSettings.bannedSongsPath + "Banned Songs.json");
            return JsonConvert.DeserializeObject<List<SongBan>>(bannedSongsString, serializerSettings);            
        }

        public void SaveBannedSongs(List<SongBan> songBans)
        {
            File.WriteAllText(filePathSettings.bannedSongsPath + "Banned Songs.json", JsonConvert.SerializeObject(songBans));
        }

        public FilesMeta LoadFilesMeta()
        {
            if (!File.Exists(filePathSettings.filesDataPath + "Files.meta")) SaveFilesMeta(new FilesMeta());
            String filesDataString = File.ReadAllText(filePathSettings.filesDataPath + "Files.meta");
            return JsonConvert.DeserializeObject<FilesMeta>(filesDataString, serializerSettings);
        }

        public void SaveFilesMeta(FilesMeta filesData)
        {
            File.WriteAllText(filePathSettings.filesDataPath + "Files.meta", JsonConvert.SerializeObject(filesData));
        }
        public List<String> LoadRankedSuggestions()
        {
            if (!File.Exists(filePathSettings.lastSuggestionsPath + "LastSuggestions.json")) SaveRankedSuggestions(new List<String>());
            String filesDataString = File.ReadAllText(filePathSettings.lastSuggestionsPath + "LastSuggestions.json");
            return JsonConvert.DeserializeObject<List<String>>(filesDataString, serializerSettings);
        }

        public void SaveRankedSuggestions(List<String> rankedSuggestions)
        {
            File.WriteAllText(filePathSettings.lastSuggestionsPath + "LastSuggestions.json", JsonConvert.SerializeObject(rankedSuggestions));
        }

        public List<String> LoadAllRankedSongs()
        {
            if (!File.Exists(filePathSettings.rankedData + "allrankedsongs.json")) SaveAllRankedSongs(new List<String>());
            String fileDataString = File.ReadAllText(filePathSettings.rankedData + "allrankedsongs.json");
            return JsonConvert.DeserializeObject<List<String>>(fileDataString, serializerSettings);
        }

        public void SaveAllRankedSongs(List<String> allRankedSongs)
        {
            File.WriteAllText(filePathSettings.rankedData + "allrankedsongs.json", JsonConvert.SerializeObject(allRankedSongs));
        }

        public FileFormatVersions LoadFileFormatVersions()
        {
            if (!File.Exists(filePathSettings.filesDataPath + "FileFormatVersions.json")) SaveFilesFormatVersions(new FileFormatVersions());
            String fileDataString = File.ReadAllText(filePathSettings.filesDataPath + "FileFormatVersions.json");
            return JsonConvert.DeserializeObject<FileFormatVersions>(fileDataString, serializerSettings);
        }

        public void SaveFilesFormatVersions(FileFormatVersions versions)
        {
            File.WriteAllText(filePathSettings.filesDataPath + "FileFormatVersions.json", JsonConvert.SerializeObject(versions));
        }
    }
}