using System;
using System.Collections.Generic;
using System.Linq;
using Actions;


namespace LinkedData
{
    public class SongEndPointCollection
    {
        public SortedDictionary<String, SongEndPoint> endPoints = new SortedDictionary<String, SongEndPoint>();
        public void SetRelevance(Actions.SongSuggest actions,int originPoints,int requiredMatches)
        {
            int percentDoneCalc = 0;
            foreach (SongEndPoint songEndPoint in endPoints.Values)
            {
                songEndPoint.SetRelevance(originPoints,requiredMatches);
                percentDoneCalc++;
                actions.songSuggestCompletion = (5.5 + (0.5 * percentDoneCalc / endPoints.Values.Count())) / 6.0;
            }
            
        }

    }
}