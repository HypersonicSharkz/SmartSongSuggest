using System;

namespace BanLike
{
    public class SongBan
    {
        public DateTime expire { get; set; }
        public DateTime activated { get; set; }
        public String songID { get; set; }
    }
}
