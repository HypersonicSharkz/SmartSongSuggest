using System;
using System.Collections.Generic;
using Settings;
using SongSuggestNS;
using PlaylistJson;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;


namespace PlaylistNS
{
    public partial class PlaylistManager
    {
        public SongSuggest songSuggest { get; set; }
        public String title { get; set; } = "Try This";
        public String author { get; set; } = "Song Suggest";
        public String image { get; set; } = "";//"data:image/png;base64,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";
        public String fileName { get; set; } = "Playlist";
        public String description { get; set; } = "";
        public String syncURL { get; set; } = "";

        //List of songID's on added songs
        public List<String> songs = new List<String>();

        public PlaylistManager(PlaylistSettings playlistSettings)
        {
            if (playlistSettings.title != null) this.title = playlistSettings.title;
            if (playlistSettings.author != null) this.author = playlistSettings.author;
            if (playlistSettings.image != null) this.image = playlistSettings.image;
            if (playlistSettings.fileName != null) this.fileName = playlistSettings.fileName;
            if (playlistSettings.description != null) this.description = playlistSettings.description;
            if (playlistSettings.syncURL != null) this.syncURL = playlistSettings.syncURL;
        }

        //Generates the playlist tekst for added songs.
        public void Generate()
        {
            String playlistText = "{";
            playlistText += "\"AllowDuplicates\":false,";
            playlistText += "\"playlistTitle\":\"" + title + "\",";
            playlistText += "\"playlistAuthor\":\"" + author + "\",";
            if (description != "") playlistText += "\"playlistDescription\":\"" + description + "\","; //Only add description if there is one
            playlistText += "\"image\":\""+image+ "\",";
            if (syncURL != "") playlistText+= "\"syncURL\":\"" + syncURL + "\","; //Only add an URL if there is one
            playlistText += "\"songs\":[";

            foreach (String song in songs)
            {
                playlistText += GetSongString(song);
            }

            playlistText = playlistText.TrimEnd(',');
            playlistText += "]}";
            songSuggest.fileHandler.SavePlaylist(playlistText, fileName);
        }

        public void LoadTest()
        {
            //Playlist playlist = songSuggest.fileHandler.LoadPlaylist("Test");
            Load("accsaber-rankedmaps-standard");
            fileName = "accsaber-rankedmaps-standardResave";
            syncURL = null;
            NewGenerate();
        }

        public void Load(String playlistFileName)
        {
            Playlist playlistJSON = songSuggest.fileHandler.LoadPlaylist(playlistFileName);
            //**Missing** Insert code to load data to this structure.

            title = playlistJSON.playlistTitle;
            author = playlistJSON.playlistAuthor;
            image = playlistJSON.image;
            description = playlistJSON.playlistDescription;
            syncURL = playlistJSON.syncURL;
            songs.Clear();

            foreach (var song in playlistJSON.songs)
            {
                foreach (var difficulty in song.difficulties)
                {
                    if (difficulty.characteristic == "Standard")
                    {
                        songs.Add(songSuggest.songLibrary.GetID(song.hash, difficulty.name));
                    }
                }
            }
        }

        public void NewGenerate()
        {
            Playlist playlist = new Playlist();

            playlist.playlistTitle = title;
            playlist.playlistAuthor = author;
            playlist.image = image;
            playlist.playlistDescription = description;
            playlist.syncURL = syncURL;

            playlist.songs = new List<SongJson>();
            foreach (var song in songs)
            {
                SongJson songJSON = new SongJson();

                songJSON.hash = songSuggest.songLibrary.GetHash(song);

                Difficulty difficultyJSON = new Difficulty();
                difficultyJSON.characteristic = "Standard";
                difficultyJSON.name =songSuggest.songLibrary.GetDifficultyName(song);

                songJSON.difficulties = new List<Difficulty>();
                songJSON.difficulties.Add(difficultyJSON);

                playlist.songs.Add(songJSON);
            }

            //**Missing** Insert code to load this objects data to the playlist before saving.

            songSuggest.fileHandler.SavePlaylist(playlist, fileName);
        }

        //Add a song to the playlist
        public void AddSong(String songID)
        {
            songs.Add(songID);
        }

        public void AddSongs(List<String> songID)
        {
            songs.AddRange(songID);
        }

        //Remove a song from the playlist
        public void RemoveSong(String songID)
        {
            songs.Remove(songID);
        }

        private String GetSongString(String songID)
        {
            string songText = "";
            songText += "{";

            songText += "\"hash\":\"" + songSuggest.songLibrary.GetHash(songID) + "\",";
            songText += "\"difficulties\":[{";
            songText += "\"characteristic\":\"Standard\",";
            songText += "\"name\":\"" + songSuggest.songLibrary.GetDifficultyName(songID) + "\"";

            songText += "}]},";

            return songText;
        }

        public List<String> GetSongs()
        {
            return songs;
        }
    }

    public enum SongSorting
    {
        Oldest = 0,
        WeightedRandom = 1
    }

}
