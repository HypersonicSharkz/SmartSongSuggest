using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaohSongSuggest.Utils
{
    public class ActivePlayer
    {
        public String id { get; set; }
        public String name { get; set; }
        public int rankedPlayCount { get; set; }
        public SortedDictionary<String, ActivePlayerScore> scores = new SortedDictionary<String, ActivePlayerScore>();

        //Returns True if Score is added or updated, False if already present.
        public Boolean AddScore(ActivePlayerScore score)
        {
            //Is song known
            if (scores.ContainsKey(score.songID))
            {
                //Select song
                ActivePlayerScore storedScore = scores[score.songID];
                //Check if it is a newer score
                if (storedScore.timeSet == score.timeSet)
                {
                    Console.WriteLine("Unchanged Score on: " + score.songID);

                    //Both scores have same timestamp, so ignore it inform requester nothing was changed.
                    return false;
                }
                else
                {
                    //Updated score ... update timestamp and pp value.
                    storedScore.timeSet = score.timeSet;
                    storedScore.pp = score.pp;
                    Console.WriteLine("Updated Score on: " + score.songID);
                }
            }
            //New score
            else
            {
                scores.Add(score.songID, score);
                Console.WriteLine("New Score on: " + score.songID);
            }

            return true;
        }

        //Tries to return a list of "count" oldest songs, or all available.
        public List<String> GetOldest(int count)
        {
            //Pull Scores into a new List from Dictionary for the playlist
            Console.WriteLine("scores available: " + scores.Count());

            List<ActivePlayerScore> candidates = new List<ActivePlayerScore>(scores.Values);

            Console.WriteLine("candidates found: " + candidates.Count());

            //Add the time of the songs and their id to a sorted list, for easy sorting on time and get the songID as output.
            SortedList<DateTime, String> candidatesList = new SortedList<DateTime, String>();
            foreach (ActivePlayerScore candidate in candidates)
            {
                candidatesList.Add(candidate.timeSet, candidate.songID);
            }

            //Get an ilist of values from the candidates list now sorted by timestamp
            List<String> candidateValues = new List<String>(candidatesList.Values);

            //select the first requested_amount candidates (or amount available if less than requested_amount ranked songs)
            return candidateValues.GetRange(0, Math.Min(count, candidateValues.Count()));
        }

        //Tries to return a list of "count" highest pp songs, or all available.
        public List<String> GetTop(int count)
        {


            //Pull Scores into a new List from Dictionary for the playlist
            List<ActivePlayerScore> candidates = new List<ActivePlayerScore>(scores.Values.OrderBy(s => s.pp).Reverse());

            //reduce candidates to count entries, or all available.
            candidates = candidates.GetRange(0, Math.Min(count, candidates.Count()));

            List<String> topList = new List<String>();
            foreach (ActivePlayerScore candidate in candidates)
            {
                topList.Add(candidate.songID);
            }

            //select the first requested_amount candidates (or amount available if less than requested_amount ranked songs)
            return topList;
        }
    }

    public class ActivePlayerScore
    {
        public String songID { get; set; }
        public DateTime timeSet { get; set; }
        public float pp { get; set; }
    }
}
