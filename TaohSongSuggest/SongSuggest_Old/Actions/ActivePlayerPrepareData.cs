using System;
using System.Linq;
using SongLibraryNS;
using ScoreSabersJson;
using ActivePlayerData;
using FileHandling;
using WebDownloading;
using DataHandling;

namespace Actions
{
    //Loads cached player data if available, or make a full data pull for a new user.
    //Parse activePlayers data via "recent" from web with Get Scores from Page 1 until a duplicate score is found
    //Checks if updated/added scores is same or higher than playcount total, if higher or equal stop (more scores could have been uploaded hency also higher)

    //Should be considered merged with Active Player ... need to decide on what is data, what is the action etc.
    public class ActivePlayerPrepareData
    {
        public ToolBox toolBox { get; set; }
        public void SetActivePlayer(String scoreSaberID)
        {
            ActivePlayer activePlayer = toolBox.activePlayer;
            FileHandler fileHandler = toolBox.fileHandler;
            WebDownloader webDownloader = toolBox.webDownloader;
            SongLibrary songLibrary = toolBox.songLibrary;

            String searchmode = "recent";
            //Load player if changed
            if (activePlayer.id != scoreSaberID)
            {
                //Console.WriteLine("Attempt to find File");
                //Console.WriteLine("Attempting to find this file name: " + activePlayerDataPath + activePlayer.id + ".json");
                //Console.WriteLine("File Found? " + (File.Exists(activePlayerDataPath + userID.Text + ".json")));

                //Check if cached data is available and load it, else prepare a new user
                if (fileHandler.ActivePlayerExist(scoreSaberID))
                {
                    activePlayer = fileHandler.LoadActivePlayer(scoreSaberID);
                    //Reset data if a new format is used.
                    if (activePlayer.OutdatedVersion())
                    {
                        activePlayer = new ActivePlayer(scoreSaberID);
                        //On a new user search by best PP scores so we can break when we hit non ranked songs.
                        searchmode = "top";
                    }
                }
                else
                {
                    activePlayer = new ActivePlayer(scoreSaberID);
                    //On a new user search by best PP scores so we can break when we hit non ranked songs.
                    searchmode = "top";
                }
            }

            //Prepare for updating from web until a duplicate score is found (then remaining scores are correct)
            int page = 0;
            string maxPage = "?";
            Boolean continueLoad = true;
            while (continueLoad)
            {
                page++;
                toolBox.status = "Downloading Player History Page: " + page + "/" + maxPage;
                Console.WriteLine("Page Start: " + page + " Search Mode: " + searchmode);
                PlayerScoreCollection playerScoreCollection = webDownloader.GetScores(activePlayer.id, searchmode, 100, page);
                maxPage = "" + Math.Ceiling((double)playerScoreCollection.metadata.total / 100);
                //PlayerScoreCollection playerScoreCollection = JsonConvert.DeserializeObject<PlayerScoreCollection>(scoresJSON, serializerSettings);
                toolBox.status = "Parsing Player History Page: " + page + "/" + maxPage;
                Console.WriteLine("Page Parse: " + page);
                //Parse Player Scores
                foreach (PlayerScore score in playerScoreCollection.playerScores)
                {
                    if (score.leaderboard.ranked)
                    {
                        //attempt to add the song to the library.
                        songLibrary.AddSong(score.leaderboard.id + "", score.leaderboard.songName, score.leaderboard.songHash, score.leaderboard.difficulty.difficulty + "");

                        //Create a score object from the website Score, and add it to the candidates
                        ActivePlayerScore tmpScore = new ActivePlayerScore
                        {
                            songID = score.leaderboard.id + "",
                            timeSet = score.score.timeSet,
                            pp = score.score.pp,
                            accuracy = 100.0 * score.score.baseScore / score.leaderboard.maxScore
                        };
                        //Attempts to add the found score, if it is a duplicate with same timestamp do not load next score page
                        //TODO: Break foreach as well
                        if (!activePlayer.AddScore(tmpScore)) continueLoad = false;
                    }
                    //break if we are doing initial user search and hit the non ranked songs
                    else if (searchmode == "top")
                    {
                        //Console.WriteLine("Hit unranked song");
                        continueLoad = false;
                    }
                }

                Console.WriteLine("Page " + page + "/" + Math.Ceiling((double)playerScoreCollection.metadata.total / 100) + " Done.");
                //Last Page check, sets loop to finish if on it.
                if (playerScoreCollection.metadata.total <= page * 100) continueLoad = false;
            }
            activePlayer.rankedPlayCount = activePlayer.scores.Count();

            //Save updated player
            fileHandler.SaveActivePlayer(activePlayer);

            //If new songs was added, save the library.
            if (songLibrary.Updated()) songLibrary.Save();

            Console.WriteLine("PlayerScores Count" + activePlayer.scores.Count());
            toolBox.activePlayer = activePlayer;
        }
    }
}

