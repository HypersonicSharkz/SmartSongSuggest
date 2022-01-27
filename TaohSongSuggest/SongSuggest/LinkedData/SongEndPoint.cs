using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;


namespace LinkedData
{
    public class SongEndPoint
    {
        public String songID { get; set; }
        public List<SongLink> songLinks = new List<SongLink>();
        public float totalRelevanceScore = 0;
        public float weightedRelevanceScore = 0;
        public int matchedSongs = 0;
        public float weightedSongs = 0;

        public void SetRelevance(int originPoints,int focusPercentage)
        {
            //Get a list of all origin songs
            List<String> originSongIDs = songLinks.Select(c => c.playerSongScore.songID).Distinct().ToList();
            matchedSongs = originSongIDs.Count();

            //Get the average of each songs links values and add them to this songs Relevance Score
            foreach (String songID in originSongIDs)
            {

                totalRelevanceScore += songLinks.Where(c => c.playerSongScore.songID == songID).Select(c => c.Strength()).Average();
            }
            //Reduces the required endpoints for maximum efficiency by a % of total endpoints
            //(e.g. 50 endpoints and 10% stylefocus means a 45+ matched song gets same score as a 50 matched song with same base weight)
            float songBonus = 0.01f * focusPercentage * originPoints;
            weightedSongs = Math.Max(0.0f + originPoints - songBonus, matchedSongs); //.  0.0f+matchedSongs+(0.0f+originPoints-matchedSongs)*(100.0f-focusPercentage)/100;
            weightedRelevanceScore = totalRelevanceScore / weightedSongs;
        }

    }
}