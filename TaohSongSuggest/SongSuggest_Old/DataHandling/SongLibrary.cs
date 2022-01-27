using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ScoreSabersJson;
using Newtonsoft.Json;
using FileHandling;
using WebDownloading;


namespace SongLibraryNS
{
    public class SongLibrary
    {
        private FileHandler fileHandler;
        private WebDownloader webDownloader;

        Boolean updated = false;
        private SortedDictionary<String, Song> songs = new SortedDictionary<string, Song>();

        public SongLibrary(FileHandler fileHandler, WebDownloader webDownloader)
        {
            this.fileHandler = fileHandler;
            this.webDownloader = webDownloader;
        }

        //Will add a song to the library if unknown, and update status to unsaved.
        public void AddSong(String scoreSaberID, String name, String hash, String difficulty)
        {
            //If song is not in the library, create it, add it, and add it to the idLookup Dictionary
            if (!songs.ContainsKey(scoreSaberID))
            {
                Song newSong = new Song
                {
                    scoreSaberID = scoreSaberID,
                    name = name,
                    hash = hash,
                    difficulty = difficulty
                };
                songs.Add(newSong.scoreSaberID, newSong);
                updated = true;
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
                GetSongInfo(scoreSaberID);
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
                GetSongInfo(scoreSaberID);
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
                GetSongInfo(scoreSaberID);
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
                GetSongInfo(hash, GetDifficultyValue(difficulty));

                foreach (Song song in songs.Values)
                {
                    if (song.hash == hash && song.difficulty == GetDifficultyValue(difficulty)) foundSong = song;
                }
            }
            return foundSong.scoreSaberID;
        }

        public void GetSongInfo(String hash, String difficultyValue)
        {
            AddSong(webDownloader.GetLeaderboardInfo(hash, difficultyValue));
            Save();
        }

        public void GetSongInfo(String scoreSaberID)
        {
            AddSong(webDownloader.GetLeaderboardInfo(scoreSaberID));
            Save();
        }

        //Adds a song via LeaderboardInfo (Does not save as many songs could be added at once).
        public void AddSong(LeaderboardInfo leaderboardInfo)
        {
            AddSong(leaderboardInfo.id + "", leaderboardInfo.songName, leaderboardInfo.songHash, leaderboardInfo.difficulty.difficulty + "");
        }

        //Returns true if songs has been added since data was loaded/library created.
        public Boolean Updated()
        {
            return updated;
        }

        //Saves an updated library
        public void Save()
        {
            fileHandler.SaveSongLibrary(GetJson());
            updated = false;
        }

        //Saves library to disk
        public String GetJson()
        {
            List<Song> JSONsongs = new List<Song>(songs.Values);
            return JsonConvert.SerializeObject(JSONsongs);
        }

        public void SetJSON(String libraryJSON)
        {
            JsonSerializerSettings serializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            List<Song> songs = JsonConvert.DeserializeObject<List<Song>>(libraryJSON, serializerSettings);
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
