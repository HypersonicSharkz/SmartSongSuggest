using Newtonsoft.Json;
using ScoreSabersJson;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TaohSongSuggest.Configuration;
using TaohSongSuggest.UI;

namespace TaohSongSuggest.Utils
{ 
    class SongSuggest
    {
        public JsonSerializerSettings serializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
        public WebClient client = new WebClient();

        private ActivePlayer activePlayer = new ActivePlayer();
        private SongLibrary songLibrary = new SongLibrary();
        private Top10kPlayers top10kPlayers = new Top10kPlayers();

        private String songLibraryPath = ConfigUtil.songLibraryPath;
        private String playListPath = ConfigUtil.playListPath;
        private String activePlayerDataPath = ConfigUtil.activePlayerDataPath;
        private String top10kPlayersPath = ConfigUtil.top10kPlayersPath;

        float progress = 0f;

        public SongSuggest()
        {
            try
            {
                String songLibraryJSON = File.ReadAllText(songLibraryPath);
                songLibrary.SetJSON(songLibraryJSON);
            }
            catch
            {
                songLibrary = new SongLibrary();
            }
        }

        //Method for sending progress info to the UI on the main thread
        public async void UpdateProgess()
        {
            while (progress != 1f) 
            {
                TSSFlowCoordinator.settingsView.RefreshProgressBar(progress);
                await Task.Delay(200);
            }
            
        }


        
        //Starts working on getting and updating the top 10k players. Just use JSON file unless data is getting old. (new songs added)
        //Top10kPlayerDataPuller();

        //Creates a playlist with 50 suggested songs based on the link system.
        public async void SuggestedSongs()
        {
            progress = 0f;

            await Task.Run(async () =>
            {
                TSSFlowCoordinator.Instance.ToggleBackButton(false);

                UpdateProgess();

                //Load Link Data
                string linkPlayerJSON;
                var assembly = Assembly.GetExecutingAssembly();

                using (Stream stream = assembly.GetManifestResourceStream(top10kPlayersPath))
                using (StreamReader reader = new StreamReader(stream))
                {
                    linkPlayerJSON = reader.ReadToEnd();
                }


                Plugin.Log.Info("Done Reading");

                Top10kPlayers players = new Top10kPlayers();
                players.SetJSON(linkPlayerJSON);

                Plugin.Log.Info("Sat JSON");

                //Load Active Player
                await SetActivePlayer();

                Plugin.Log.Info("Loaded Player");

                Stopwatch timer = new Stopwatch();
                timer.Start();

                //Create a List of songID's to not target with links
                //Current filters is known songs, could be replaced with a function for what a player wants to filter, adding dislike songs etc.
                List<String> ignoreSongs = new List<String>(activePlayer.scores.Keys);

                //All Song Endpoints seen from the origin (Players) point of view.
                SongEndPointCollection originSongs = new SongEndPointCollection();

                //Create PlayerOriginEndPoints for top 50 songs
                foreach (String songID in activePlayer.GetTop(50))
                {
                    SongEndPoint songEndPoint = new SongEndPoint { songID = songID };
                    originSongs.endPoints.Add(songID, songEndPoint);

                    progress += (1f / 50) * 0.1f;
                }
                progress = 0.6f;

                Plugin.Log.Info("OriginEndpoint gotten");

                List<Top10kPlayer> sortedTop10K = players.top10kPlayers.Where(player => player.id != activePlayer.id && player.rank >= SettingsController.cfgInstance.fromRank && player.rank <= SettingsController.cfgInstance.toRank).ToList();
                //Prepare the starting endpoints for the above selected songs and tie them to the origin collection, ignoring the player itself.
                foreach (Top10kPlayer player in sortedTop10K)
                {
                    progress += (1f / sortedTop10K.Count) * 0.2f;
                    //Loop all preselected origin songs on a player
                    foreach (Top10kScore playerSong in player.top10kScore.Where(playerSong => originSongs.endPoints.ContainsKey(playerSong.songID)))
                    {
                        //Loop songs again for endpoints, skipping linking itself, as well as ignoreSongs
                        foreach (Top10kScore suggestedSong in player.top10kScore.Where(suggestedSong => suggestedSong.rank != playerSong.rank && !ignoreSongs.Contains(suggestedSong.songID)))
                        {
                            SongLink songLink = new SongLink
                            {
                                playerID = player.id,
                                playerSongScore = playerSong,
                                suggestedSongScore = suggestedSong
                            };
                            originSongs.endPoints[playerSong.songID].songLinks.Add(songLink);
                        }
                    }
                }
                progress = 0.8f;

                Plugin.Log.Info("Done Starting endpoints");

                //Create the suggested songs Endpoints
                SongEndPointCollection suggestedSongs = new SongEndPointCollection();

                //loop all origin songs
                foreach (SongEndPoint songEndPoint in originSongs.endPoints.Values)
                {
                    progress += (1f / originSongs.endPoints.Values.Count) * 0.1f;
                    //loop all links in that active origin song
                    foreach (SongLink songLink in songEndPoint.songLinks)
                    {
                        //If song is not present, make an endpoint for it
                        if (!suggestedSongs.endPoints.ContainsKey(songLink.suggestedSongScore.songID))
                        {
                            SongEndPoint suggestedSongEndPoint = new SongEndPoint { songID = songLink.suggestedSongScore.songID };
                            suggestedSongs.endPoints.Add(songLink.suggestedSongScore.songID, suggestedSongEndPoint);
                        }

                        //add endpoint to suggested song
                        suggestedSongs.endPoints[songLink.suggestedSongScore.songID].songLinks.Add(songLink);
                    }
                }
                progress = 0.9f;

                Plugin.Log.Info("Looped all origin songs");

                //Calculate the scores on the songs for suggestions
                suggestedSongs.SetRelevance();

                //Find most relevant songs for playlist selection
                List<SongEndPoint> candidates = suggestedSongs.endPoints.Values.OrderByDescending(s => s.relevanceScore).ToList();
                candidates = candidates.GetRange(0, Math.Min(50, candidates.Count()));

                //Make Playlist
                Playlist playList = new Playlist(songLibrary);
                playList.AddSongs(candidates.Select(c => c.songID).ToList());
                File.WriteAllText(playListPath, JsonConvert.SerializeObject(JsonConvert.DeserializeObject(playList.Generate()), Formatting.Indented));

                Plugin.Log.Info("Made Playlist");

                timer.Stop();
                Plugin.Log.Info("Time Spent: " + timer.ElapsedMilliseconds);

                progress = 1f;

                

                TSSFlowCoordinator.Instance.ToggleBackButton(true);
            });

            /*
            //Add each songs in a playlist by name to the output window
            output.Text = "";

            //Add each songs in a playlist by name to the output window
            foreach (String songID in playList.GetSongs())
            {
                output.Text += songLibrary.getName(songID) + "(" + songLibrary.getDifficultyName(songID) + ")" + Environment.NewLine;
            }
            */

            //float totalScore = linkScore.Sum(link => link.Stength());
            //Console.WriteLine("Total Score: " + totalScore);
        }

        //Creates a playlist with the 100 oldest maps for a player.
        public void Oldest100ActivePlayer()
        {
            //Create empty playlist, and reset output window.
            Playlist playList = new Playlist(songLibrary);
            //output.Text = "";

            //Update the active players data from the score page.
            SetActivePlayer();

            //Add up to 100 oldest song to playlist
            playList.AddSongs(activePlayer.GetOldest(100));

            //Generate and save a playlist with the selected songs in the playlist.
            File.WriteAllText(playListPath, playList.Generate());

            /*
            //Add each songs in a playlist by name to the output window
            foreach (String songID in playList.GetSongs())
            {
                output.Text += songLibrary.getName(songID) + "(" + songLibrary.getDifficultyName(songID) + ")" + Environment.NewLine;
            }*/
        }

        public void Top10kPlayerDataPuller()
        {
            //Delete/Rename the json file if you want a new set of 10k players, else data on 10k known players is kept.
            if (File.Exists(top10kPlayersPath))
            {
                string top10kPlayerJSON = File.ReadAllText(top10kPlayersPath);
                top10kPlayers.SetJSON(top10kPlayerJSON);
            }
            else
            {
                Retrieve10kTopPlayers();
            }
            Console.WriteLine(top10kPlayers.top10kPlayers.Count());
            //Set true if you want scores updates on the players
            Retrieve10kTopPlayersScores(true);
        }

        //Retrieves the 10k top players 1 page (50) at a time.
        public void Retrieve10kTopPlayers()
        {

            Stopwatch rateLimiter = new Stopwatch();
            for (int i = 1; i <= 200; i++)
            {
                rateLimiter.Start();
                PlayerCollection players = JsonConvert.DeserializeObject<PlayerCollection>(GetPlayers(i), serializerSettings);
                foreach (Player player in players.players)
                {
                    top10kPlayers.Add(player.id + "", player.name, player.rank);
                }
                rateLimiter.Stop();
                Console.WriteLine(rateLimiter.ElapsedMilliseconds);
                if ((int)rateLimiter.ElapsedMilliseconds < 160) Thread.Sleep(160 - (int)rateLimiter.ElapsedMilliseconds);
                rateLimiter.Reset();
            }

            File.WriteAllText(top10kPlayersPath, top10kPlayers.GetJSON());
            Console.WriteLine(top10kPlayers.top10kPlayers.Count);
            Console.WriteLine(top10kPlayers.top10kPlayers[0].name);



        }

        //Loops the known players and updates their score, if Update is True scores are updated regardless of content, else only missing players data
        public void Retrieve10kTopPlayersScores(Boolean updateAll)
        {
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
                    File.WriteAllText(top10kPlayersPath, top10kPlayers.GetJSON());
                    if (songLibrary.Updated()) File.WriteAllText(songLibraryPath, songLibrary.GetJson());
                }

                //Clear scores if all should be updated.
                if (updateAll) player.top10kScore.Clear();

                if (player.top10kScore.Count() < 20)
                {
                    //Max 1 request per 160ms to keep rate limit under 400
                    rateLimiter.Start();
                    String scoresJSON = GetScores(player.id, "top", 20, 1).Result;
                    rateLimiter.Stop();
                    if ((int)rateLimiter.ElapsedMilliseconds < 160) Thread.Sleep(160 - (int)rateLimiter.ElapsedMilliseconds);
                    rateLimiter.Reset();
                    PlayerScoreCollection playerScoreCollection = JsonConvert.DeserializeObject<PlayerScoreCollection>(scoresJSON, serializerSettings);

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
            File.WriteAllText(top10kPlayersPath, top10kPlayers.GetJSON());
        }
        //Pulls activePlayers data from web with Get Scores Page 1, gets the amount of available ranked plays and go through pages until
        //Checks if updated/added scores is same or higher than playcount total, if higher or equal stop (more scores could have been uploaded hency also higher)
        public async Task SetActivePlayer()
        {
            //Load player if changed
            //Check if cached data is available and load it, else prepare a new user
            Plugin.Log.Info("SAP 1");
            if (File.Exists(activePlayerDataPath))
            {
                String activePlayerString = File.ReadAllText(activePlayerDataPath);
                activePlayer = JsonConvert.DeserializeObject<ActivePlayer>(activePlayerString, serializerSettings);
            }
            else
            {
                activePlayer = new ActivePlayer();
                activePlayer.id = BS_Utils.Gameplay.GetUserInfo.GetUserID();
            }
            Plugin.Log.Info("SAP 2");

            //Prepare for updating from web until a duplicate score is found (then remaining scores are correct)
            int page = 0;
            Boolean continueLoad = true;
            while (continueLoad)
            {
                page++;
                Plugin.Log.Info("Page Start: " + page);
                String scoresJSON = await GetScores(activePlayer.id, "recent", 100, page);
                PlayerScoreCollection playerScoreCollection = JsonConvert.DeserializeObject<PlayerScoreCollection>(scoresJSON, serializerSettings);
                Plugin.Log.Info("Page Parse: " + page);

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
                            pp = score.score.pp
                        };
                        //Attempts to add the found score, if it is a duplicate with same timestamp do not load next score page
                        //TODO: Break foreach as well
                        if (!activePlayer.AddScore(tmpScore)) continueLoad = false;
                    }
                }

                Plugin.Log.Info("Page " + page + "/" + Math.Ceiling((double)playerScoreCollection.metadata.total / 100) + " Done.");
                progress = ((float)page / (float)Math.Ceiling((double)playerScoreCollection.metadata.total / 100)) / 2f;
                //Last Page check, sets loop to finish if on it.
                if (playerScoreCollection.metadata.total <= page * 100) continueLoad = false;
            }
            activePlayer.rankedPlayCount = activePlayer.scores.Count();

            //Save updated player
            File.WriteAllText(activePlayerDataPath, JsonConvert.SerializeObject(activePlayer));

            //If new songs was added, save the library.
            if (songLibrary.Updated())
            {
                //Saves the updated Song Library
                File.WriteAllText(songLibraryPath, songLibrary.GetJson());
                songLibrary.SetSaved();
            }
            Plugin.Log.Info("PlayerScores Count" + activePlayer.scores.Count());
            progress = 0.5f;
        }

        //Generic web puller for scores
        private async Task<String> GetScores(String id, String sorting, int count, int page)
        {
            try
            {
                //Console.WriteLine("Starting page " + page);

                //https://scoresaber.com/api/player/76561197993806676/scores?limit=20&sort=recent&page=2
                return await client.DownloadStringTaskAsync("https://scoresaber.com/api/player/" + id + "/scores?limit=" + count + "&sort=" + sorting + "&page=" + page);
            }
            catch
            {
                Plugin.Log.Info("Error on " + page);
            }
            return "";
        }

        //Generic web puller for top players
        private String GetPlayers(int page)
        {
            try
            {
                Plugin.Log.Info("Starting page " + page);

                //https://scoresaber.com/api/players?page=2
                return client.DownloadString("https://scoresaber.com/api/players?page=" + page);
            }
            catch
            {
                Plugin.Log.Info("Error on " + page);
            }
            return "";
        }

    }
}
