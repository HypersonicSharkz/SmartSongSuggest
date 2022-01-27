using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaohSongSuggest.Utils
{
    public class SongEndPointCollection
    {
        public SortedDictionary<String, SongEndPoint> endPoints = new SortedDictionary<String, SongEndPoint>();
        public void SetRelevance()
        {
            foreach (SongEndPoint songEndPoint in endPoints.Values)
            {
                songEndPoint.SetRelevance();
            }
        }

    }
    public class SongEndPoint
    {
        public String songID { get; set; }
        public List<SongLink> songLinks = new List<SongLink>();
        public float relevanceScore = 0;
        public int matchedSongs = 0;

        public void SetRelevance()
        {
            //Get a list of all origin songs
            List<String> originSongIDs = songLinks.Select(c => c.playerSongScore.songID).Distinct().ToList();
            matchedSongs = originSongIDs.Count();

            //Get the average of each songs links values and add them to this songs Relevance Score
            foreach (String songID in originSongIDs)
            {
                relevanceScore += songLinks.Where(c => c.playerSongScore.songID == songID).Select(c => c.Strength()).Average();
            }
        }

    }
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
