using System;
using System.Collections.Generic;
using System.Linq;
using SongLibraryNS;
using FileHandling;

namespace BanLike
{
    public class SongLiking
    {
        public SongLibrary songLibrary { get; set; }
        public FileHandler fileHandler { get; set; }
        public List<SongLike> likedSongs = new List<SongLike>();

        public List<String> GetLikedIDs()
        {
            return likedSongs.Select(p => p.songID).ToList();
        }

        public SongLiking(SongLibrary songLibrary)
        {
            this.songLibrary = songLibrary;
        }

        //Returns true if Liked
        public Boolean IsLiked(String songHash, String difficulty)
        {
            String songID = songLibrary.GetID(songHash, difficulty);
            return likedSongs.Any(p => p.songID == songID);
        }

        public void RemoveLike(String songHash, String difficulty)
        {
            String songID = songLibrary.GetID(songHash, difficulty);
            likedSongs.RemoveAll(p => p.songID == songID);
            Save();
        }

        public void SetLike(String songHash, String difficulty)
        {
            //If a Like is in place, remove it before setting the new Like.
            if (IsLiked(songHash, difficulty)) RemoveLike(songHash, difficulty);
            likedSongs.Add(new SongLike { activated = DateTime.UtcNow, songID = songLibrary.GetID(songHash, difficulty) });
            Save();
        }

        public void Save()
        {
            fileHandler.SaveLikedSongs(likedSongs);
        }
    }
}
