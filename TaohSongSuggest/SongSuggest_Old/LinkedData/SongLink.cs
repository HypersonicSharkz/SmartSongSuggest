using System;


namespace LinkedData
{
    public class SongLink
    {
        public String playerID { get; set; }
        public Top10kScore playerSongScore { get; set; }
        public Top10kScore suggestedSongScore { get; set; }
        private int strength;
        private bool unset = true;

        public float Strength()
        {
            if (unset)
            {
                strength = Math.Max((20 - Math.Abs(playerSongScore.rank - suggestedSongScore.rank)), 1);
                unset = false;
            }
            return strength;
        }
    }
}