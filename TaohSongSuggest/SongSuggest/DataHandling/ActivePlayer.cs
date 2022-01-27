using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ActivePlayerData
{
    public class ActivePlayer
    {
        private String currentVersion = "1.0";
        public String currentSavedVersion;
        public String id { get; set; }
        public String name { get; set; }
        public int rankedPlayCount { get; set; }
        public SortedDictionary<String, ActivePlayerScore> scores = new SortedDictionary<String, ActivePlayerScore>();

        public ActivePlayer (String scoreSaberID)
        {
            id = scoreSaberID;
            currentSavedVersion = currentVersion;
        }

        public Boolean OutdatedVersion()
        {
            return !(currentSavedVersion == currentVersion);
        }

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
                    //Console.WriteLine("Unchanged Score on: " + score.songID);

                    //Both scores have same timestamp, so ignore it inform requester nothing was changed.
                    return false;
                }
                else
                {
                    //Updated score ... update timestamp and pp value.
                    storedScore.timeSet = score.timeSet;
                    storedScore.pp = score.pp;
                    //Console.WriteLine("Updated Score on: " + score.songID);
                }
            }
            //New score
            else
            {
                scores.Add(score.songID, score);
                //Console.WriteLine("New Score on: " + score.songID);
            }

            return true;
        }

        //Tries to return a list of "count" oldest songs, or all available.
        public List<String> GetOldest(int count, double accuracy, int days)
        {
            //Pull Scores into a new List from Dictionary for the playlist
            Console.WriteLine("scores available: " + scores.Count());

            List<ActivePlayerScore> candidates = new List<ActivePlayerScore>(scores.Values);

            Console.WriteLine("candidates found: " + candidates.Count());

            //Add the time of the songs and their id to a sorted list, for easy sorting on time and get the songID as output.
            SortedList<DateTime, String> candidatesList = new SortedList<DateTime, String>();
            //Only grab the songs with an accuracy lower than the cuttoff level.
            foreach (ActivePlayerScore candidate in candidates.Where(c => c.accuracy < accuracy && c.timeSet < DateTime.UtcNow.AddDays(-days)))
            {
                Console.WriteLine(candidate.accuracy + " " + accuracy);
                candidatesList.Add(candidate.timeSet, candidate.songID);
            }

            //Get an ilist of values from the candidates list now sorted by timestamp
            List<String> candidateValues = new List<String>(candidatesList.Values);

            //select the first requested_amount candidates (or amount available if less than requested_amount ranked songs)
            return candidateValues.GetRange(0, Math.Min(count, candidateValues.Count()));
        }

        //Returns a list of Ranked songs older than the selected days
        public List<String> GetYoungerThan(int days)
        {
            DateTime target = DateTime.UtcNow.AddDays(-days);
            List<String> answer = new List<String>();
            foreach (ActivePlayerScore candidate in new List<ActivePlayerScore>(scores.Values))
            {
                if (candidate.timeSet > target) answer.Add(candidate.songID);
            }
            return answer;
        }

        //Tries to return a list of "count" highest pp songs, or all available. Negative values are legal, and treated as request 0
        public List<String> GetTop(int count)
        {
            //Sanity check, returns an empty list if a negative amount is requested.
            if (count < 0) count = 0;

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
}
