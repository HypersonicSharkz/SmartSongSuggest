using System;
using System.Linq;
using SongLibraryNS;
using ScoreSabersJson;
using ActivePlayerData;
using WebDownloading;
using SongSuggestNS;

namespace Actions
{
    //Loads cached player data if available, or make a full data pull for a new user.
    //Parse activePlayers data via "recent" from web with Get Scores from Page 1 until a duplicate score is found
    //Checks if updated/added scores is same or higher than playcount total, if higher or equal stop (more scores could have been uploaded hency also higher)

    //Should be considered merged with Active Player ... need to decide on what is data, what is the action etc.
    public class ActivePlayerRefreshData
    {
        public SongSuggest songSuggest { get; set; }
        public void RefreshActivePlayer()
        {
            ActivePlayer activePlayer = songSuggest.activePlayer;
            WebDownloader webDownloader = songSuggest.webDownloader;
            SongLibrary songLibrary = songSuggest.songLibrary;

            //Have the active player verify the active version and the wanted version is the same, else load any cached version or present an empty user
            activePlayer.LoadActivePlayer(songSuggest);

            //Figure out which searchmode to use. If 0 count songs, go through all ranked, else update via recent
            String searchmode = (songSuggest.activePlayer.rankedPlayCount == 0) ? "top" : "recent";

            //Prepare for updating from web until a duplicate score is found (then remaining scores are correct)
            int page = 0;
            string maxPage = "?";
            Boolean continueLoad = true;
            while (continueLoad)
            {
                page++;
                songSuggest.status = "Downloading Player History Page: " + page + "/" + maxPage;
                Console.WriteLine("Page Start: " + page + " Search Mode: " + searchmode);
                PlayerScoreCollection playerScoreCollection = webDownloader.GetScores(activePlayer.id, searchmode, 100, page);
                maxPage = ""+Math.Ceiling((double)playerScoreCollection.metadata.total / 100);
                //PlayerScoreCollection playerScoreCollection = JsonConvert.DeserializeObject<PlayerScoreCollection>(scoresJSON, serializerSettings);
                songSuggest.status = "Parsing Player History Page: " + page + "/" + maxPage;
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
                            accuracy = 100.0*score.score.baseScore / score.leaderboard.maxScore
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
            activePlayer.Save();

            //If new songs was added, save the library.
            if (songLibrary.Updated()) songLibrary.Save();

            Console.WriteLine("PlayerScores Count" + activePlayer.scores.Count());
            songSuggest.activePlayer = activePlayer;
        }
    }
}
 
