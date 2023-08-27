using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using ScoreSabersJson;
using SongSuggestNS;

namespace SongLibraryNS
{
    public class SongLibrary
    {
        public SongSuggest songSuggest { get; set; }

        Boolean updated = false;
        public SortedDictionary<String, Song> songs = new SortedDictionary<String, Song>();

        //Will add a song to the library if unknown, and update status to unsaved.
        public void AddSong(String scoreSaberID, String name, String hash, String difficulty, double starBeatSaber)
        {
            Song newSong = new Song
            {
                scoreSaberID = scoreSaberID,
                name = name,
                hash = hash,
                starBeatSaber = starBeatSaber,
                difficulty = difficulty
            };
            AddSong(newSong);
        }

        //Add a Song Object to the Libary
        public void AddSong(Song song)
        {
            //If song is not in the library, create it, add it, and add it to the idLookup Dictionary
            if (!songs.ContainsKey(song.scoreSaberID))
            {
                songs.Add(song.scoreSaberID, song);
                updated = true;
            }
        }

        //Adds a song via LeaderboardInfo (Does not save as many songs could be added at once).
        public void AddSong(LeaderboardInfo leaderboardInfo)
        {
            AddSong(leaderboardInfo.id + "", leaderboardInfo.songName, leaderboardInfo.songHash, leaderboardInfo.difficulty.difficulty + "", leaderboardInfo.stars);
        }

        //Adds a new song based only on (Leaderboard) SongID
        public void AddSong(String scoreSaberID)
        {
            //Only add if not there, if not there get WebInfo and pass it on.
            if (!songs.ContainsKey(scoreSaberID))
            {
                AddSong(songSuggest.webDownloader.GetLeaderboardInfo(scoreSaberID));
                Save();
            }
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

        //Returns the ID of a known song, or search web.
        public String GetID(String hash, String difficulty)
        {
            Song foundSong = null;
            //Try and find the songs information and return it from library
            foreach (Song song in songs.Values)
            {
                if (song.hash.ToUpperInvariant() == hash.ToUpperInvariant() && song.difficulty == GetDifficultyValue(difficulty)) foundSong = song;
            }

            //If the song was not found, try pulling info from web and then find it
            if (foundSong == null)
            {
                //Add missing song from web data, and try and find information again
                WebGetSongInfo(hash, GetDifficultyValue(difficulty));
                foreach (Song song in songs.Values)
                {
                    if (song.hash.ToUpperInvariant() == hash.ToUpperInvariant() && song.difficulty == GetDifficultyValue(difficulty)) foundSong = song;
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

        //Checks if a song is in the Library
        public Boolean Contains(String hash, String difficulty)
        {
            Boolean foundSong = false;
            //Try and find the songs information and return if it was in the library.
            foreach (Song song in songs.Values)
            {
                if (song.hash.ToUpperInvariant() == hash.ToUpperInvariant() && song.difficulty == GetDifficultyValue(difficulty)) foundSong = true;
            }
            return foundSong;
        }


        //Adds a new library to the current Libary (used when new Web Data is downloaded)
        public void AddLibrary(List<Song> songs)
        {
            foreach (Song song in songs)
            {
                //Check if song is known
                if (this.songs.ContainsKey(song.scoreSaberID))
                {
                    //Replace the stored values with the new songs info (if changed).
                    ReplaceSong(song);
                }
                else
                {
                    //Add unknown songs
                    AddSong(song);
                }
            }

            //Save if any changes was made
            if (Updated()) Save();
        }

        //Removes songs that are not actively linked to a supported format
        public void RemoveSongsWithoutSongCategories()
        {
            List<String> songsToRemove = songs.Values.Where(c => c.songCategory == 0).Select(c => c.scoreSaberID).ToList();

            if (songsToRemove.Count > 0) updated = true;

            foreach (var id in songsToRemove)
            {
                songs.Remove(id);
            }

            if (Updated()) Save();
        }

        //Replace a song in the songs list if there has been an update in any status.
        private void ReplaceSong(Song song)
        {
            var librarySong = songs[song.scoreSaberID];

            bool identical = true;

            Type type = typeof(Song);
            PropertyInfo[] properties = type.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                object value1 = property.GetValue(song);
                object value2 = property.GetValue(librarySong);

                if (!Equals(value1, value2)) identical = false;
            }

            if (!identical)
            {
                songSuggest.log?.WriteLine($"Updated: {song.scoreSaberID} As new song values was given.");
                songs[song.scoreSaberID] = song;
                updated = true;
            }
        }

        //Activate SongType/s
        public void AddSongCategory(String songID, SongCategory songCategory)
        {
            Console.WriteLine($"Adding {songCategory} from {songID} ");
            var song = songs[songID];

            //Check if update is needed and then perform it.
            if ((song.songCategory & songCategory) != songCategory)
            {
                updated = true;
                song.songCategory = songCategory | song.songCategory;
            }
        }

        //Deactives SongType/s
        public void RemoveSongCategory(String songID, SongCategory songCategory)
        {
            Console.WriteLine($"Removing {songCategory} from {songID} ");
            var song = songs[songID];

            // Check if update is needed and then perform it.
            if ((song.songCategory & songCategory) != 0)
            {
                updated = true;
                song.songCategory = song.songCategory & (~songCategory);
            }
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

        //Returns True/False if hte song is recorded with an active SongCategory
        public bool HasSongCategory(String songID)
        {
            return (songs.ContainsKey(songID) && songs[songID].songCategory != 0);
        }

        //Returns True/False if hte song is recorded with an active SongCategory
        public bool HasSongCategory(String hash, String difficulty)
        {
            return HasSongCategory(GetID(hash,difficulty));
        }

        //Return IDs of all known ranked songs
        public List<String> GetAllRankedSongIDs(SongCategory songCategory)
        {
            return songs.Where(c => (c.Value.songCategory & songCategory) != 0).Select(c => c.Key).ToList();
        }

        //Return IDs of all known ranked songs from the given list
        public List<String> GetAllRankedSongIDs(SongCategory songCategory, List<String> songs)
        {
            return this.songs.Where(c => (c.Value.songCategory & songCategory) != 0).Select(c => c.Key).Intersect(songs).ToList();
        }

        //Translate the difficulty name with the assigned value.
        public String GetDifficultyValue(String difficultyText)
        {
            difficultyText = difficultyText.ToLowerInvariant();
            switch (difficultyText)
            {
                case "easy":
                    return "1";
                case "normal":
                    return "3";
                case "hard":
                    return "5";
                case "expert":
                    return "7";
                case "expert+":
                    return "9";
                //Playlists Expert+ reference
                case "expertplus":
                    return "9";
                default:
                    return "0";
            }
        }
    }
}