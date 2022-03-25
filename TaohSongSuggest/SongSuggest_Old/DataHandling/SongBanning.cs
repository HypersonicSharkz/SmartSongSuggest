using System;
using System.Collections.Generic;
using System.Linq;
using SongSuggestNS;

namespace BanLike
{
    public class SongBanning
    {
        public SongSuggest songSuggest { get; set; }
        public List<SongBan> bannedSongs = new List<SongBan>();

        public List<String> GetBannedIDs()
        {
            return bannedSongs.Where(p => p.expire > DateTime.UtcNow).Select(p => p.songID).ToList();
        }

        public List<String> GetPermaBannedIDs()
        {
            return bannedSongs.Where(p => p.expire == DateTime.MaxValue).Select(p => p.songID).ToList();
        }


        public Boolean IsBanned(String songHash, String difficulty)
        {
            String songID = songSuggest.songLibrary.GetID(songHash, difficulty);
            return bannedSongs.Any(p => p.songID == songID && p.expire > DateTime.UtcNow);
        }

        public void LiftBan(String songHash, String difficulty)
        {
            String songID = songSuggest.songLibrary.GetID(songHash, difficulty);
            bannedSongs.RemoveAll(p => p.songID == songID);
            Save();
        }

        public void SetBan(String songHash, String difficulty, int days)
        {
            //If a ban is in place, remove it before setting the new ban.
            if (IsBanned(songHash, difficulty)) LiftBan(songHash, difficulty);
            bannedSongs.Add(new SongBan { expire = DateTime.UtcNow.AddDays(days), activated = DateTime.UtcNow, songID = songSuggest.songLibrary.GetID(songHash, difficulty) });
            Save();
        }

        public void SetPermaBan(String songHash, String difficulty)
        {
            //If a ban is in place, remove it before setting the new ban.
            if (IsBanned(songHash, difficulty)) LiftBan(songHash, difficulty);
            bannedSongs.Add(new SongBan { expire = DateTime.MaxValue, activated = DateTime.UtcNow, songID = songSuggest.songLibrary.GetID(songHash, difficulty) });
            Save();
        }

        public void Save()
        {
            songSuggest.fileHandler.SaveBannedSongs(bannedSongs.Where(p => p.expire > DateTime.UtcNow).ToList());
        }
    }
}
