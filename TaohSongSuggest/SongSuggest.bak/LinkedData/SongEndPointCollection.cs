using System;
using System.Collections.Generic;
using System.Linq;
using SongSuggestNS;


namespace LinkedData
{
    public class SongEndPointCollection
    {
        public SortedDictionary<String, SongEndPoint> endPoints = new SortedDictionary<String, SongEndPoint>();
        public void SetRelevance(Actions.RankedSongsSuggest actions,int originPoints,int requiredMatches)
        {
            int percentDoneCalc = 0;
            foreach (SongEndPoint songEndPoint in endPoints.Values)
            {
                songEndPoint.SetRelevance(originPoints,requiredMatches);
                percentDoneCalc++;
                actions.songSuggestCompletion = (5.5 + (0.5 * percentDoneCalc / endPoints.Values.Count())) / 6.0;
            }
        }

        public void SetDistance(SongSuggest songSuggest)
        {
            foreach (SongEndPoint songEndPoint in endPoints.Values)
            {
                songEndPoint.SetDistance(songSuggest);
            }
        }

        public void SetStyle(SongEndPointCollection originSongs)
        {
            foreach (SongEndPoint songEndPoint in endPoints.Values)
            {
                songEndPoint.SetStyle(originSongs);
            }
        }

        public void SetPP(SongSuggest songSuggest)
        {
            foreach (SongEndPoint songEndPoint in endPoints.Values)
            {
                songEndPoint.SetPP(songSuggest);
            }
        }

        public void SetLocalPP(SongSuggest songSuggest)
        {
            foreach (SongEndPoint songEndPoint in endPoints.Values)
            {
                songEndPoint.SetLocalPP(songSuggest);
            }
        }
    }
}