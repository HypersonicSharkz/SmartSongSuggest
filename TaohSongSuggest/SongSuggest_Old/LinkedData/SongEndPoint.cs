using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using SongSuggestNS;

namespace LinkedData
{
    public class SongEndPoint
    {
        public String songID { get; set; }
        public List<SongLink> songLinks = new List<SongLink>();
        public float totalRelevanceScore = 0;
        public double totalRank = 0;
        public float weightedRelevanceScore = 0;
        public int matchedSongs = 0;
        public float weightedSongs = 0;
        public double averageRank = 0;

        //Total of all unique grouping of Origin -> Endpoint links distance averages.
        public double totalDistance = 0;
        public double averageDistance = 0;

        //StyleFilter data
        public double proportionalStyle = 0;

        public void SetRelevance(int originPoints,int requiredMatches)
        {
            //Get a list of all origin songs
            List<String> originSongIDs = songLinks.Select(c => c.originSongScore.songID).Distinct().ToList();
            matchedSongs = originSongIDs.Count();

            //Get the average of each songs links values and add them to this songs Relevance Score
            foreach (String songID in originSongIDs)
            {
                totalRelevanceScore += songLinks.Where(c => c.originSongScore.songID == songID).Select(c => c.Strength()).Average();
            //    totalRank += songLinks.Where(c => c.playerSongScore.songID == songID).Select(c => c.suggestedSongScore.rank).Average();
            }
            //Reduces the required endpoints for maximum efficiency by a % of total endpoints
            //(e.g. 50 endpoints and 90% requiredMatches means a 45+ matched song gets same score as a 50 matched song with same base weight)
            float songBonus = 0.01f * (100.0f-requiredMatches) * originPoints;
            weightedSongs = Math.Max(0.0f + originPoints-songBonus, matchedSongs); //.  0.0f+matchedSongs+(0.0f+originPoints-matchedSongs)*(100.0f-focusPercentage)/100;
            weightedRelevanceScore = totalRelevanceScore / weightedSongs;
            //averageRank = totalRank / matchedSongs;

            //Calculate averageRank with a minimum link amount, center vs 10.50 (1+2+...20)/20
            float minRankLinks = 50;
            float rankSum = songLinks.Select(c => c.targetSongScore.rank).Sum();
            float rankLinks = songLinks.Count;
            rankSum += Math.Max(minRankLinks - rankLinks, 0.0f)*10.5f;
            averageRank = rankSum / Math.Max(minRankLinks, rankLinks);

        }

        //Average of the songLinks (per song) distance. Require the Players Origin Song score to set 1,1 coordinates.
        public void SetDistance (SongSuggest songSuggest)
        {
            List<String> originSongIDs = songLinks.Select(c => c.originSongScore.songID).Distinct().ToList();
            matchedSongs = originSongIDs.Count();
            foreach (String songID in originSongIDs)
            {
                totalDistance += songLinks.Where(c => c.originSongScore.songID == songID).Select(c => c.Distance(songSuggest)).Average();
            }
            averageDistance = totalDistance / matchedSongs;
        }

        public void SetStyle(SongEndPointCollection originSongs)
        {
            List<String> originSongIDs = songLinks.Select(c => c.originSongScore.songID).Distinct().ToList();
            foreach (String originSongID in originSongIDs)
            {
                int originSongCount = originSongs.endPoints[originSongID].songLinks.Count();
                int linkedCount = songLinks.Select(c => c.originSongScore.songID == songID).Count();
                proportionalStyle += 1.0*linkedCount/originSongCount;
            }
        }
    }
}