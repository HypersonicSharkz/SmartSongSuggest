using Data;
using DataHandling;
using FileHandling;
using LinkedData;
using ScoreSabersJson;
using SongLibraryNS;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using WebDownloading;

namespace Actions
{

    class Top10kRefresh
    {
        public ToolBox toolBox { get; set; }
        private Top10kPlayers top10kPlayers;
        
        public void Top10kPlayerDataPuller(Boolean fullRefresh)
        {
            FileHandler fileHandler = toolBox.fileHandler;
            //WebDownloader webDownloader = toolBox.webDownloader;

            //If partial refresh, current data is loaded, else a new dataset is made.
            if (fileHandler.LinkedDataExist()&&!fullRefresh)
            {
                Console.WriteLine("Keeping Users");
                //Current players and their score data is loaded, and the score data is cleared
                top10kPlayers = fileHandler.LoadLinkedData();
                foreach (Top10kPlayer player in top10kPlayers.top10kPlayers)
                {
                    player.top10kScore.Clear();
                }
            }
            else
            {
                Console.WriteLine("New Users");
                //Make fresh dataset and get the players
                top10kPlayers = new Top10kPlayers();
                Retrieve10kTopPlayers();
            }
            Console.WriteLine(top10kPlayers.top10kPlayers.Count());
            //Set true if you want scores updates on the players
            Retrieve10kTopPlayersScores();
            UpdateFilesMeta();
        }

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
        public void Retrieve10kTopPlayersScores()
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

        public void UpdateFilesMeta()
        {
            FilesMeta filesMeta = toolBox.fileHandler.LoadFilesMeta();
            String oldVersion = filesMeta.top10kVersion;
            int seperatorLocation = oldVersion.IndexOf(".");
            String before = oldVersion.Substring(0, seperatorLocation);
            String after = oldVersion.Substring(seperatorLocation+1);
            String updatedAfter = ""+(int.Parse(after)+1);
            String newVersion = before+"."+updatedAfter;
            filesMeta.top10kUpdated = DateTime.UtcNow;
            filesMeta.top10kVersion = newVersion;
            toolBox.fileHandler.SaveFilesMeta(filesMeta);
        }
    }
}