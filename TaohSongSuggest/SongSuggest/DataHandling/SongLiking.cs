using System;
using System.Collections.Generic;
using System.Linq;
using SongLibraryNS;
using SongSuggestNS;

namespace BanLike
{
    public class SongLiking
    {
        public SongSuggest songSuggest { get; set; }
        
        public List<SongLike> likedSongs = new List<SongLike>();

        public List<String> GetLikedIDs()
        {
            return likedSongs.Select(p => p.songID).ToList();
        }

        //Returns true if Liked
        public Boolean IsLiked(String songHash, String difficulty)
        {
            String songID = songSuggest.songLibrary.GetID(songHash, difficulty);
            return IsLiked(songID);
        }

        public Boolean IsLiked(String songID)
        {
            return likedSongs.Any(p => p.songID == songID);
        }

        public void RemoveLike(String songHash, String difficulty)
        {
            String songID = songSuggest.songLibrary.GetID(songHash, difficulty);
            RemoveLike(songID);
        }
        public void RemoveLike(String songID)
        {
            likedSongs.RemoveAll(p => p.songID == songID);
            Save();
        }

        public void SetLike(String songHash, String difficulty)
        {
            String songID = songSuggest.songLibrary.GetID(songHash, difficulty);
            SetLike(songID);
        }
        public void SetLike(String songID)
        {
            //If a Like is in place, remove it before setting the new Like.
            if (IsLiked(songID)) RemoveLike(songID);
            likedSongs.Add(new SongLike { 
                activated = DateTime.UtcNow, 
                songID = songID 
            });
            Save();
        }


        public void Save()
        {
            songSuggest.fileHandler.SaveLikedSongs(likedSongs);
        }
    }
}
