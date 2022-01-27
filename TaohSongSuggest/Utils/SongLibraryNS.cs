using Newtonsoft.Json;
using SongLibraryNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaohSongSuggest.Utils
{
    public class SongLibrary
    {
        Boolean updated = false;
        private SortedDictionary<String, Song> songs = new SortedDictionary<string, Song>();

        public SongLibrary()
        {
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

        public String getName(String scoreSaberID)
        {
            return songs[scoreSaberID].name;
        }

        public String getHash(String scoreSaberID)
        {
            Plugin.Log.Info(scoreSaberID);
            return songs[scoreSaberID].hash;
        }

        public String getDifficultyName(String scoreSaberID)
        {
            return songs[scoreSaberID].getDifficultyText();
        }

        //Returns true if songs has been added since data was loaded/library created.
        public Boolean Updated()
        {
            return updated;
        }

        //Updates class to be in a saved state
        public void SetSaved()
        {
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

    }
}
