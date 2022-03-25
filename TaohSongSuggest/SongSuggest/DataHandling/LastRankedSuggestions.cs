using System;
using System.Collections.Generic;

namespace SongSuggestNS
{
    public class LastRankedSuggestions
    {
        public SongSuggest songSuggest { get; set; }
        public List<String> lastSuggestions { get; set; }

        public void Load()
        {
            lastSuggestions = songSuggest.fileHandler.LoadRankedSuggestions();
        }

        public void Save()
        {
            songSuggest.fileHandler.SaveRankedSuggestions(lastSuggestions);
        }

        public String GetRank(String hash, String difficulty)
        {
            String id = songSuggest.songLibrary.Contains(hash, difficulty) ? songSuggest.songLibrary.GetID(hash, difficulty) : "";
            return lastSuggestions.Contains(id)?""+(lastSuggestions.IndexOf(id)+1):"";
        }

        public String GetRankCount()
        {
            return "" + lastSuggestions.Count;
        }
    }
}
