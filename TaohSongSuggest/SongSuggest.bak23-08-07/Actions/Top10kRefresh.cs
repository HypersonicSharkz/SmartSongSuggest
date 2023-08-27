using Data;
using SongSuggestNS;
using FileHandling;
using LinkedData;
using ScoreSabersJson;
using SongLibraryNS;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using WebDownloading;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Actions
{

    class Top10kRefresh
    {
        public SongSuggest songSuggest { get; set; }
        private Top10kPlayers top10kPlayers;
        
        public void Top10kPlayerDataPuller(Boolean fullRefresh)
        {
            FileHandler fileHandler = songSuggest.fileHandler;
            //Makes a new top10kPlayers, either to keep empty or to load data from disk.
            top10kPlayers = new Top10kPlayers() { songSuggest = songSuggest};

            //If partial refresh, current data is loaded, else a new dataset is made.
            if (fileHandler.LinkedDataExist()&&!fullRefresh)
            {
                songSuggest.log?.WriteLine("Keeping Users");
                //Current players and their score data is loaded, and the score data is cleared
                top10kPlayers.Load();
                foreach (Top10kPlayer player in top10kPlayers.top10kPlayers)
                {
                    player.top10kScore.Clear();
                }
            }
            else
            {
                songSuggest.log?.WriteLine("New Users");
                //Make fresh dataset and get the players
                Retrieve10kTopPlayers();
            }
            songSuggest.log?.WriteLine(top10kPlayers.top10kPlayers.Count());

            //Generate and Save PlayerScores and Meta file
            //Set true if you want scores updates on the players
            Retrieve10kTopPlayersScores();
            UpdateFilesMeta(); //Meta file for when this data was created, not local meta on top 10k players

            //Update songSuggests reference to the new top10kPlayers.
            top10kPlayers.GenerateTop10kSongMeta();
            songSuggest.top10kPlayers = top10kPlayers;
        }

        //Retrieves the 10k top players 1 page (50) at a time.
        public void Retrieve10kTopPlayers()
        {
            FileHandler fileHandler = songSuggest.fileHandler;
            WebDownloader webDownloader = songSuggest.webDownloader;

            Stopwatch rateLimiter = new Stopwatch();
            for (int i = 1; i <= 200; i++)
            {
                rateLimiter.Start();
                PlayerCollection players = webDownloader.GetPlayers(i);
                foreach (Player player in players.players)
                {
                    top10kPlayers.Add(player.id + "", player.name, player.rank);
                }
                rateLimiter.Stop();
                songSuggest.log?.WriteLine("{0}ms   Players Parsed: {1}",rateLimiter.ElapsedMilliseconds, top10kPlayers.top10kPlayers.Count);
                if ((int)rateLimiter.ElapsedMilliseconds < 160) Thread.Sleep(160 - (int)rateLimiter.ElapsedMilliseconds);
                rateLimiter.Reset();
            }
            top10kPlayers.Save();
            songSuggest.log?.WriteLine(top10kPlayers.top10kPlayers.Count);
            songSuggest.log?.WriteLine(top10kPlayers.top10kPlayers[0].name);
        }



        //Loops the known players and updates their score, if Update is True scores are updated regardless of content, else only missing players data
        public void Retrieve10kTopPlayersScores()
        {
            SongLibrary songLibrary = songSuggest.songLibrary;
            WebDownloader webDownloader = songSuggest.webDownloader;

            int totalCount = 0;
            Stopwatch rateLimiter = new Stopwatch();
            songSuggest.log?.WriteLine("Starting loads");
            foreach (Top10kPlayer player in top10kPlayers.top10kPlayers)
            {

                totalCount++;
                if (totalCount % 100 == 0)
                {
                    songSuggest.log?.WriteLine("Working on player: " + totalCount);
                    //Saves files every 100 entries
                    top10kPlayers.Save();
                    if (songLibrary.Updated()) songLibrary.Save();
                }

                if (player.top10kScore.Count() < 20)
                {
                    //Max 1 request per 160ms to keep rate limit under 400
                    rateLimiter.Start();
                    PlayerScoreCollection playerScoreCollection = webDownloader.GetScores(player.id, "top", 20, 1);
                    rateLimiter.Stop();
                    if ((int)rateLimiter.ElapsedMilliseconds < 160) Thread.Sleep(160 - (int)rateLimiter.ElapsedMilliseconds);
                    rateLimiter.Reset();
                    //PlayerScoreCollection playerScoreCollection = JsonConvert.DeserializeObject<PlayerScoreCollection>(scoresJSON, serializerSettings);

                    //Resets the counter for derived Rank of song
                    int i = 0;
                    foreach (PlayerScore score in playerScoreCollection.playerScores)
                    {

                        if (score.leaderboard.ranked)
                        {
                            songLibrary.AddSong(score.leaderboard.id + "", score.leaderboard.songName, score.leaderboard.songHash, score.leaderboard.difficulty.difficulty + "",score.leaderboard.stars);
                            Top10kScore tmpScore = new Top10kScore();
                            tmpScore.songID = score.leaderboard.id + "";
                            tmpScore.pp = score.score.pp;
                            i++;
                            tmpScore.rank = i;
                            player.top10kScore.Add(tmpScore);
                        }
                    }
                }
            }
            top10kPlayers.Save();
        }

        public void UpdateFilesMeta()
        {
            FilesMeta filesMeta = songSuggest.fileHandler.LoadFilesMeta();
            String oldVersion = filesMeta.top10kVersion;
            int seperatorLocation = oldVersion.IndexOf(".", StringComparison.Ordinal);
            String before = oldVersion.Substring(0, seperatorLocation);
            String after = oldVersion.Substring(seperatorLocation+1);
            String updatedAfter = ""+(int.Parse(after)+1);
            String newVersion = before+"."+updatedAfter;
            filesMeta.top10kUpdated = DateTime.UtcNow;
            filesMeta.top10kVersion = newVersion;
            songSuggest.fileHandler.SaveFilesMeta(filesMeta);
        }

        //Grabs top 30 plays for each players
        //Filters out players with too large a distance between 1st and 30th play
        //Filter out the 10 plays that are the lowest acc (likely too hard)
        internal void AlternativeTop10kPlayerDataPuller(ref Top10kPlayers fullInfoPlayers)
        {
            double maxSpread = 0.7; //closer to 1 the lower the spread

            //FileHandler fileHandler = songSuggest.fileHandler;
            WebDownloader webDownloader = songSuggest.webDownloader;
            SongLibrary songLibrary = songSuggest.songLibrary;
            Throttler throttler = webDownloader.ssThrottler;
            
            top10kPlayers = new Top10kPlayers() { songSuggest = songSuggest };

            //counters of progress
            int playerCount = 0;
            int skippedPlayers = 0;
            int lowPlayCount = 0;
            List<string> lowPlayCountID = new List<string>();
            int lowEfficiencyPlayers = 0;
            List<string> lowEfficiencyPlayersID = new List<string>();
            int inactivePlayers = 0;
            List<string> inactivePlayersID = new List<string>();
            int candidatePage = 1;


            throttler.Call();
            List<Player> candidates = webDownloader.GetPlayers(candidatePage++).players.ToList();
            songSuggest.log?.WriteLine("Starting to Download Users");

            //Continue until 10k valid players are found (or too many players are skipped)
            while (playerCount < 10000 || skippedPlayers > 10000)
            {
                //Update on progress
                if ((playerCount + skippedPlayers) % 100 == 0) songSuggest.log?.WriteLine("Found Users: {0} Skipped Users: {1} ({2} low efficiency / {3} low play / {4} inactive)", playerCount, skippedPlayers, lowEfficiencyPlayers, lowPlayCount, inactivePlayers);

                //Grab a new batch of players if out of players
                if (candidates.Count() == 0)
                {
                    throttler.Call();
                    candidates = webDownloader.GetPlayers(candidatePage++).players.ToList();
                }

                //Get next Candidate.
                var candidate = candidates.First();
                candidates.Remove(candidate);

                //Lets Generate a player based on the candidate and validate it
                var currentPlayer = new Top10kPlayer()
                {
                    id = "" + candidate.id,
                    name = candidate.name,
                    rank = playerCount+1 //For compatibility we need to keep ranges from 1 to 10k players, so consider rank as rank of the 10k approved players
                };

                //Lets grab the top 30 scores of that player and get them added
                throttler.Call();
                PlayerScoreCollection playerScoreCollection = webDownloader.GetScores(currentPlayer.id, "top", 30, 1);

                //Let us create a Top10k score for each of these, and make a list sorted by their acc (else that data is lost)
                List<(float acc, Top10kScore score)> accList = new List<(float, Top10kScore)>();

                DateTime newestScore = DateTime.MinValue;

                int index = 0;
                foreach (PlayerScore score in playerScoreCollection.playerScores)
                {
                    if (score.leaderboard.ranked)
                    {
                        songLibrary.AddSong(score.leaderboard.id + "", score.leaderboard.songName, score.leaderboard.songHash, score.leaderboard.difficulty.difficulty + "",score.leaderboard.stars);
                        Top10kScore tmpScore = new Top10kScore();
                        tmpScore.songID = score.leaderboard.id + "";
                        tmpScore.pp = score.score.pp;


                        score.acc = 1.0f * score.score.modifiedScore / score.leaderboard.maxScore;
                        float rankPercentile = score.score.rank/score.leaderboard.plays;

                        (float, Top10kScore) item = (score.acc, tmpScore);
                        accList.Add(item);

                        if (score.score.timeSet > newestScore) newestScore = score.score.timeSet;
                    }
                }

                //If a return of all players item has been made, store player in it before filtering.
                if (fullInfoPlayers != null)
                {
                    Top10kPlayer fullInfoPlayer = new Top10kPlayer()
                    {
                        id = "" + candidate.id,
                        name = candidate.name,
                        rank = playerCount + skippedPlayers + 1
                    };
                    foreach (var score in accList.Select(c => c.score))
                    {
                        fullInfoPlayer.top10kScore.Add(score);
                    }
                    fullInfoPlayers.top10kPlayers.Add(fullInfoPlayer);
                }

                //If the player does not have 30 scores skip the player
                if (accList.Count() < 30)
                {
                    skippedPlayers++;
                    lowPlayCount++;
                    lowPlayCountID.Add(""+candidate.id);
                    continue;
                }

                //If the player has not set a new score in 90 days skip the player
                if (DateTime.UtcNow.Subtract(newestScore).TotalDays > 365)
                {
                    skippedPlayers++;
                    inactivePlayers++;
                    inactivePlayersID.Add("" + candidate.id);
                    continue;
                }

                //Let us get the 20 best scores by acc
                var scores = accList.OrderByDescending(c => c.acc).Take(20).ToList();

                scores = scores.OrderByDescending(c => c.score.pp).ToList();

                //If spread on players pp is too large skip
                double playerSpread = scores.Last().score.pp/scores.First().score.pp;
                if (playerSpread < maxSpread)
                {
                    skippedPlayers++;
                    lowEfficiencyPlayers++;
                    lowEfficiencyPlayersID.Add("" + candidate.id);
                    continue;
                }

                //Set the scores rank by PP and add them to the player
                int scoreRank = 1;
                foreach (var score in scores.Select(c=> c.score).OrderByDescending(c => c.pp))
                {
                    score.rank = scoreRank;
                    currentPlayer.top10kScore.Add(score);
                    scoreRank++;
                }

                //Add the player to the list of players
                top10kPlayers.top10kPlayers.Add(currentPlayer);
                playerCount++;
                
                
            }
            //Update on progress
            if ((playerCount + skippedPlayers) % 100 == 0) songSuggest.log?.WriteLine("Found Users: {0} Skipped Users: {1} ({2} low efficiency / {3} low play / {4} inactive)", playerCount, skippedPlayers, lowEfficiencyPlayers, lowPlayCount, inactivePlayers);


            if (songLibrary.Updated()) songLibrary.Save();
            UpdateFilesMeta();
            top10kPlayers.Save();
            top10kPlayers.GenerateTop10kSongMeta();
            songSuggest.top10kPlayers = top10kPlayers;

            //Console.WriteLine("Low Play Count");
            //foreach(string id in lowPlayCountID)
            //{
            //    Console.WriteLine(id);
            //}

            //Console.WriteLine("Low Efficiency");
            //foreach (string id in lowEfficiencyPlayersID)
            //{
            //    Console.WriteLine(id);
            //}

            //Console.WriteLine("Inactive");
            //foreach (string id in inactivePlayersID)
            //{
            //    Console.WriteLine(id);
            //}
        }
    }
}