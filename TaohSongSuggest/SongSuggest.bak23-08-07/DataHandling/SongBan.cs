using System;

namespace BanLike
{
    public enum BanType
    {
        SongSuggest,    //Banned in the Suggestions
        Oldest,      //Banned in the Oldest Songs
        AccSaber        //Banned from the AccSaber list
    }
    public class SongBan
    {
        public DateTime expire { get; set; }
        public DateTime activated { get; set; }
        public String songID { get; set; }
        public BanType banType { get; set; }
    }
}
