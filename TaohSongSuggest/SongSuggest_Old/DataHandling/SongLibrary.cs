using System;
using System.Collections.Generic;
using ScoreSabersJson;
using SongSuggestNS;

namespace SongLibraryNS
{
    public class SongLibrary
    {
        public SongSuggest songSuggest { get; set; }

        Boolean updated = false;
        private SortedDictionary<String, Song> songs = new SortedDictionary<String, Song>();

        //Will add a song to the library if unknown, and update status to unsaved.
        public void AddSong(String scoreSaberID, String name, String hash, String difficulty)
        {
            Song newSong = new Song
            {
                scoreSaberID = scoreSaberID,
                name = name,
                hash = hash,
                difficulty = difficulty
            };
            AddSong(newSong);
        }

        public String GetName(String scoreSaberID)
        {
            try
            {
                return songs[scoreSaberID].name;
            }
            //Song is not in library, lets try pulling info from web
            catch
            {
                WebGetSongInfo(scoreSaberID);
                return songs[scoreSaberID].name;
            }
        }

        public String GetHash(String scoreSaberID)
        {
            try
            {
                return songs[scoreSaberID].hash;
            }
            //Song is not in library, lets try pulling info from web
            catch
            {
                WebGetSongInfo(scoreSaberID);
                return songs[scoreSaberID].hash;
            }
        }

        public String GetDifficultyName(String scoreSaberID)
        {
            try
            {
                return songs[scoreSaberID].GetDifficultyText();
            }
            //Song is not in library, lets try pulling info from web
            catch
            {
                WebGetSongInfo(scoreSaberID);
                return songs[scoreSaberID].GetDifficultyText();
            }
        }

        //Returns the ID of a known song, or searcher web.
        public String GetID(String hash, String difficulty)
        {
            Song foundSong = null;
            //Try and find the songs information and return it from library
            foreach (Song song in songs.Values)
            {
                if (song.hash == hash && song.difficulty == GetDifficultyValue(difficulty)) foundSong = song ;
            }

            //If the song was not found, try pulling info from web and then find it
            if (foundSong == null)
            {
                //Add missing song from web data, and try and find information again
                WebGetSongInfo(hash, GetDifficultyValue(difficulty));
                foreach (Song song in songs.Values)
                {
                    if (song.hash == hash && song.difficulty == GetDifficultyValue(difficulty)) foundSong = song;
                }
            }
            return foundSong.scoreSaberID;
        }

        public void WebGetSongInfo(String hash, String difficultyValue)
        {
            AddSong(songSuggest.webDownloader.GetLeaderboardInfo(hash, difficultyValue));
            Save();
        }

        public void WebGetSongInfo(String scoreSaberID)
        {
            AddSong(songSuggest.webDownloader.GetLeaderboardInfo(scoreSaberID));
            Save();
        }

        //Adds a song via LeaderboardInfo (Does not save as many songs could be added at once).
        public void AddSong(LeaderboardInfo leaderboardInfo)
        {
            AddSong(leaderboardInfo.id + "", leaderboardInfo.songName, leaderboardInfo.songHash, leaderboardInfo.difficulty.difficulty + "");
        }

        //Checks if a song is in the Library
        public Boolean Contains(String hash, String difficulty)
        {
            Boolean foundSong = false;
            //Try and find the songs information and return if it was in the library.
            foreach (Song song in songs.Values)
            {
                if (song.hash == hash && song.difficulty == GetDifficultyValue(difficulty)) foundSong = true;
            }
            return foundSong;
        }

        public void AddSong(Song song)
        {
            //If song is not in the library, create it, add it, and add it to the idLookup Dictionary
            if (!songs.ContainsKey(song.scoreSaberID))
            {
                songs.Add(song.scoreSaberID, song);
                updated = true;
            }
        }

        //Adds a new library to the current Libary (used when new Web Data is downloaded)
        public void AddLibrary(List<Song> songs)
        {
            foreach (Song song in songs)
            {
                AddSong(song);
            }
            if (Updated()) Save();
        }

        //Returns true if songs has been added since data was loaded/library created.
        public Boolean Updated()
        {
            return updated;
        }

        //Saves an updated library
        public void Save()
        {
            songSuggest.fileHandler.SaveSongLibrary(new List<Song>(songs.Values));
            updated = false;
        }

        //Sets the library to the current list of songs.
        public void SetLibrary(List<Song> songs)
        {
            foreach (Song song in songs)
            {
                this.songs.Add(song.scoreSaberID, song);
            }
        }

        //Translate the difficulty name with the assigned value.
        public String GetDifficultyValue(String difficultyText)
        {
            switch (difficultyText)
            {
                case "Easy":
                    return "1";
                case "Normal":
                    return "3";
                case "Hard":
                    return "5";
                case "Expert":
                    return "7";
                //GUIs Expert+ reference
                case "Expert+":
                    return "9";
                //Playlists Expert+ reference
                case "ExpertPlus":
                    return "9";
                default:
                    return "1";
            }
        }
    }
}