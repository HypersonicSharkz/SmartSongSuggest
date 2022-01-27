namespace ScoreSabersJson
{
    public class Player
    {
        public long id { get; set; }
        public string name { get; set; }
        public string profilePicture { get; set; }
        public string country { get; set; }
        public float pp { get; set; }
        public int rank { get; set; }
        public int countryRank { get; set; }
        public string role { get; set; }
        public Badge[] badges { get; set; }
        public string histories { get; set; }
        public ScoreStats scoreStats { get; set; }
        public int permissions { get; set; }
        public bool banned { get; set; }
        public bool inactive { get; set; }
    }
}


