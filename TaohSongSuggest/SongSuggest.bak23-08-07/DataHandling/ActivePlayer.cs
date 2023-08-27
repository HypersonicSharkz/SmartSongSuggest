using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using SongSuggestNS;

namespace ActivePlayerData
{
    public class ActivePlayer
    {
        private SongSuggest songSuggest;
        private String currentVersion = "2.0";
        public String currentSavedVersion { get; set; }
        public String id { get; set; }
        public String name { get; set; }
        public int rankedPlayCount { get; set; }
        public SortedDictionary<String, ActivePlayerScore> scores = new SortedDictionary<String, ActivePlayerScore>();

        //No selected user (json extract or new user)
        public ActivePlayer()
        {
        }

        //If there has been a user switch (and user is not -1) load user, else make an empty object the user from drive if cache is available and correct version, else creates a new user.
        public void LoadActivePlayer(SongSuggest songSuggest)
        {
            //First chance to save the active songSuggest, as loaded instances do not have one, and it is kept private to avoid 
            //saving it ... need to seperate data and actions to avoid this.
            this.songSuggest = songSuggest;

            //Find the ID that was requested.
            String requestedID = songSuggest.activePlayerID;
            String currentID = id;

            //Update the ID of the active user, then figure out if there is cached data to load.
            id = requestedID;

            //Load data if requested user has changed to a new valid user (not -1) and attempt to load the user, else keep the current.
            if (currentID != requestedID && requestedID != "-1")
            {
                songSuggest.log?.WriteLine("Attempting to Load Player");
                ActivePlayer loadedPlayer = songSuggest.fileHandler.LoadActivePlayer(requestedID);
                //Verify the loadedPlayer is in correct format.
                if (loadedPlayer.currentSavedVersion == currentVersion)
                {
                    //Move Data into this player
                    name = loadedPlayer.name;
                    rankedPlayCount = loadedPlayer.rankedPlayCount;
                    scores = loadedPlayer.scores;
                    currentSavedVersion = loadedPlayer.currentSavedVersion;
                }
                else
                {
                    //Leave data empty but update version to current, and set the user ID so it can be saved, and save the newly generated user.
                    currentSavedVersion = currentVersion;
                    Save();
                }
            }
            else
            {
                songSuggest.log?.WriteLine("Correct player was loaded or -1 user");
            }
            //Once data is updated, set the current cached users to this.
        }

        public void Save()
        {
            songSuggest.fileHandler.SaveActivePlayer(this, id);
        }

        public Boolean OutdatedVersion()
        {
            songSuggest.log?.WriteLine("Secret Version: " + currentVersion);
            songSuggest.log?.WriteLine("Disk Version: " + currentSavedVersion);
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
                    //songSuggest.log?.WriteLine("Unchanged Score on: " + score.songID);

                    //Both scores have same timestamp, so ignore it inform requester nothing was changed.
                    return false;
                }
                else
                {
                    songSuggest.log?.WriteLine("Updated Score on: "+score.songID);
                    //Updated score ... update timestamp and pp value.
                    storedScore.timeSet = score.timeSet;
                    storedScore.pp = score.pp;
                    storedScore.accuracy = score.accuracy;
                    storedScore.rankPercentile = score.rankPercentile;
                    storedScore.rankScoreSaber = score.rankScoreSaber;
                    //songSuggest.log?.WriteLine("Updated Score on: " + score.songID);
                }
            }
            //New score
            else
            {
                scores.Add(score.songID, score);
                //songSuggest.log?.WriteLine("New Score on: " + score.songID);
            }

            return true;
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

        public void ResetScores()
        {
            rankedPlayCount = 0;
            scores = new SortedDictionary<String, ActivePlayerScore>();
        }

        public double GetScore(string songID)
        {
            return scores.Keys.Contains(songID) ? scores[songID].pp : -1.0;
        }
    }
}
