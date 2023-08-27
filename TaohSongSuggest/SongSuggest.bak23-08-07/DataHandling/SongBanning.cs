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
            return bannedSongs.Where(p => p.expire > DateTime.UtcNow).Select(p => p.songID).Distinct().ToList();
        }

        public List<String> GetBannedIDs(BanType banType)
        {
            return bannedSongs
                .Where(p => p.expire > DateTime.UtcNow)
                .Where(p => p.banType == banType)
                .Select(p => p.songID)
                .Distinct()
                .ToList();
        }

        //Returns a list of all Permabanned Songs (this is from ALL BanTypes, as they all are set at max expire)
        public List<String> GetPermaBannedIDs()
        {
            return bannedSongs.Where(p => p.expire == DateTime.MaxValue).Select(p => p.songID).Distinct().ToList();
        }
        public List<String> GetPermaBannedIDs(BanType banType)
        {
            return bannedSongs.Where(p => p.expire == DateTime.MaxValue && p.banType == banType).Select(p => p.songID).Distinct().ToList();
        }

        public Boolean IsBanned(String songHash, String difficulty)
        {
            String songID = songSuggest.songLibrary.GetID(songHash, difficulty);
            return IsBanned(songID);
        }
        public Boolean IsBanned(String songID)
        {
            return IsBanned(songID, BanType.SongSuggest);
        }
        public Boolean IsBanned(String songID, BanType banType)
        {
            return bannedSongs.Any(p => p.songID == songID && p.expire > DateTime.UtcNow && p.banType == banType);
        }


        public Boolean IsPermaBanned(String songHash, String difficulty)
        {
            return IsPermaBanned(songSuggest.songLibrary.GetID(songHash, difficulty));
        }
        public Boolean IsPermaBanned(String songID)
        {
            return IsPermaBanned(songID, BanType.SongSuggest);
        }
        public Boolean IsPermaBanned(String songID, BanType banType)
        {
            return bannedSongs.Any(p => p.songID == songID && p.expire == DateTime.MaxValue && p.banType == banType);
        }

        public void LiftBan(String songHash, String difficulty)
        {
            String songID = songSuggest.songLibrary.GetID(songHash, difficulty);
            LiftBan(songID);
        }

        public void LiftBan(String songID)
        {
            LiftBan(songID, BanType.SongSuggest);
        }

        public void LiftBan(String songID, BanType banType)
        {
            bannedSongs.RemoveAll(p => p.songID == songID && p.banType == banType);
            Save();
        }

        public void SetBan(String songHash, String difficulty, int days)
        {
            //If a ban is in place, remove it before setting the new ban.
            if (IsBanned(songHash, difficulty)) LiftBan(songHash, difficulty);
            bannedSongs.Add(new SongBan
            {
                expire = DateTime.UtcNow.AddDays(days),
                activated = DateTime.UtcNow,
                songID = songSuggest.songLibrary.GetID(songHash, difficulty),
                banType = BanType.SongSuggest
            });
            Save();
        }

        public void SetPermaBan(String songHash, String difficulty)
        {
            String songID = songSuggest.songLibrary.GetID(songHash, difficulty);
            SetPermaBan(songID);
        }
        public void SetPermaBan(String songID)
        {
            SetPermaBan(songID, BanType.SongSuggest);
        }
        public void SetPermaBan(String songID, BanType banType)
        {
            //If a ban is in place, remove it before setting the new ban.
            if (IsBanned(songID, banType)) LiftBan(songID, banType);
            bannedSongs.Add(new SongBan
            {
                expire = DateTime.MaxValue,
                activated = DateTime.UtcNow,
                songID = songID,
                banType = banType
            });
            Save();
        }


        public void Save()
        {
            songSuggest.fileHandler.SaveBannedSongs(bannedSongs.Where(p => p.expire > DateTime.UtcNow).ToList());
        }
    }
}
