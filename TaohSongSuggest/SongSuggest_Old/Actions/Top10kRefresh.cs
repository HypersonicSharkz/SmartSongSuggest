using System;
using System.Linq;
using System.Threading;
using SongLibraryNS;
using ScoreSabersJson;
using System.Diagnostics;
using LinkedData;
using FileHandling;
using WebDownloading;
using DataHandling;

namespace Actions
{
    class Top10kRefresh
    {
        //Should potentially be moved to the ToolBox
        private Top10kPlayers top10kPlayers = new Top10kPlayers();
        public void Top10kPlayerDataPuller()
        {
            FileHandler fileHandler = toolBox.fileHandler;
            WebDownloader webDownloader = toolBox.webDownloader;

            //Delete/Rename the json file if you want a new set of 10k players, else data on 10k known players is kept.
            if (fileHandler.LinkedDataExist())
            {
                //string top10kPlayerJSON = File.ReadAllText(top10kPlayersPath);
                top10kPlayers = fileHandler.LoadLinkedData(); //.SetJSON(top10kPlayerJSON);
            }
            else
            {
                Retrieve10kTopPlayers();
            }
            Console.WriteLine(top10kPlayers.top10kPlayers.Count());
            //Set true if you want scores updates on the players
            Retrieve10kTopPlayersScores(true);
        }
        public ToolBox toolBox { get; set; }

        //Retrieves the 10k top players 1 page (50) at a time.
        public void Retrieve10kTopPlayers()
        {
            FileHandler fileHandler = toolBox.fileHandler;
            WebDownloader webDownloader = toolBox.webDownloader;

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
                Console.WriteLine(rateLimiter.ElapsedMilliseconds);
                if ((int)rateLimiter.ElapsedMilliseconds < 160) Thread.Sleep(160 - (int)rateLimiter.ElapsedMilliseconds);
                rateLimiter.Reset();
            }
            fileHandler.SaveLinkedData(top10kPlayers.GetJSON());
            //File.WriteAllText(top10kPlayersPath, top10kPlayers.GetJSON());
            Console.WriteLine(top10kPlayers.top10kPlayers.Count);
            Console.WriteLine(top10kPlayers.top10kPlayers[0].name);
        }



        //Loops the known players and updates their score, if Update is True scores are updated regardless of content, else only missing players data
        public void Retrieve10kTopPlayersScores(Boolean updateAll)
        {
            SongLibrary songLibrary = toolBox.songLibrary;
            FileHandler fileHandler = toolBox.fileHandler;
            WebDownloader webDownloader = toolBox.webDownloader;

            int totalCount = 0;
            Stopwatch rateLimiter = new Stopwatch();
            Console.WriteLine("Starting loads");
            foreach (Top10kPlayer player in top10kPlayers.top10kPlayers)
            {

                totalCount++;
                if (totalCount % 100 == 0)
                {
                    Console.WriteLine("Working on player: " + totalCount);
                    //Saves files every 100 entries
                    fileHandler.SaveLinkedData(top10kPlayers.GetJSON());
                    if (songLibrary.Updated()) songLibrary.Save();
                }

                //Clear scores if all should be updated.
                if (updateAll) player.top10kScore.Clear();

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
                            songLibrary.AddSong(score.leaderboard.id + "", score.leaderboard.songName, score.leaderboard.songHash, score.leaderboard.difficulty.difficulty + "");
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
            fileHandler.SaveLinkedData(top10kPlayers.GetJSON());
        }
    }
}