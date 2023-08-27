using System;
using System.Collections.Generic;


namespace PlaylistJson
{
    public class Playlist
    {
        public bool AllowDuplicates { get; set; }
        public string playlistTitle { get; set; }
        public string playlistAuthor { get; set; }
        public string image { get; set; }
        public string syncURL { get; set; }
        public string playlistDescription { get; set; }
        public List<SongJson> songs { get; set; }
    }
}
