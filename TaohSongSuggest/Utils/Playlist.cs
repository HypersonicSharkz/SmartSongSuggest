using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaohSongSuggest.Utils
{
    public class Playlist
    {
        public String title { get; set; } = "Try This";
        public String author { get; set; } = "Song Suggest";
        public String image { get; set; } = "base64,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";
        private SongLibrary songLibrary;

        //List of songID's on added songs
        List<String> songs = new List<String>();

        public Playlist(SongLibrary songLibrary)
        {
            this.songLibrary = songLibrary;
        }

        //Generates the playlist tekst for added songs.
        public String Generate()
        {
            String playlistText = "{";
            playlistText += "\"AllowDuplicates\": false,";
            playlistText += "\"playlistTitle\":\"" + title + "\",";
            playlistText += "\"playlistAuthor\":\"" + author + "\",";
            playlistText += "\"image\":\"" + image + "\",";
            playlistText += "\"songs\":[";

            foreach (String song in songs)
            {
                playlistText += GetSongString(song);
            }

            playlistText = playlistText.TrimEnd(',');
            playlistText += "]}";
            return playlistText;
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

            songText += "\"hash\":\"" + songLibrary.getHash(songID) + "\",";
            songText += "\"difficulties\":[{";
            songText += "\"characteristic\":\"Standard\",";
            songText += "\"name\":\"" + songLibrary.getDifficultyName(songID) + "\"";

            songText += "}]},";

            return songText;
        }

        public List<String> GetSongs()
        {
            return songs;
        }
    }
}
