using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using SongLibraryNS;
using PlaylistNS;
using ActivePlayerData;
using System.Diagnostics;
using LinkedData;
using FileHandling;
using Settings;
using BanLike;
using DataHandling;

namespace Actions
{
    public class SongSuggest
    {
        public double songSuggestCompletion { get; set; }
        public ActivePlayer activePlayer { get; set; }
        public SongLibrary songLibrary { get; set; }
        public FileHandler fileHandler { get; set; }
        public SongBanning songBanning { get; set; }
        public SongLiking songLiking { get; set; }
        public List<String> songSuggestIDs { get; set; }
        private ToolBox toolBox;

        public SongSuggest(ToolBox toolBox)
        {
            this.toolBox = toolBox;
            fileHandler = toolBox.fileHandler;
            activePlayer = toolBox.activePlayer;
            songLibrary = toolBox.songLibrary;
            songBanning = toolBox.songBanning;
            songLiking = toolBox.songLiking;
        }


        //Creates a playlist with 50 suggested songs based on the link system.
        public void SuggestedSongs(SongSuggestSettings settings)
        {
            toolBox.SetActivePlayer(settings.scoreSaberID);
            activePlayer = toolBox.activePlayer;
             //Load Link Data
            Top10kPlayers players = fileHandler.LoadLinkedData();

            Stopwatch timer = new Stopwatch();
            timer.Start();

            toolBox.status = "Preparing Ignore List";
            //Create a List of songID's to not target with links
            //Current filters is known songs, could be replaced with a function for what a player wants to filter, adding dislike songs etc.
            List<String> ignoreSongs = new List<String>();
            
            //Add either all played songs
            if (settings.ignorePlayedAll)
            {
                ignoreSongs.AddRange(activePlayer.scores.Keys);
            }
            //Or the songs only played within a given time periode
            else
            {
                ignoreSongs.AddRange(activePlayer.GetYoungerThan(settings.ignorePlayedDays));
            }

            //Add the banned songs to the ignoresong list if not already on it.
            ignoreSongs = ignoreSongs.Union(songBanning.GetBannedIDs()).ToList();

            //Set the limits of the links to use player songs from. Keeping all links might be better.
            int playerRankFrom = settings.rankFrom;
            int playerRankTo = settings.rankTo;


            toolBox.status = "Preparing Origin Songs";
            //Create PlayerOriginEndPoints for top 50 songs
            List<String> originSongsIDs = new List<String>();

            //Add Liked songs.
            Console.WriteLine("Use Liked Songs: " + settings.useLikedSongs);

            if (settings.useLikedSongs) originSongsIDs = originSongsIDs.Union(songLiking.GetLikedIDs()).ToList();

            Console.WriteLine("Songs in list: " + originSongsIDs.Count());

            //Fill list to 50 if liked songs are not used, or the selection is made to fill
            if (!settings.useLikedSongs || settings.fillLikedSongs) originSongsIDs = originSongsIDs.Union(activePlayer.GetTop(50-originSongsIDs.Count())).ToList();

            Console.WriteLine("Songs in list: " + originSongsIDs.Count());

            //If no songs are added (user has 0 ranked plays) a "random" list is generated, selected top 10k players and random songs from their lists.
            if (originSongsIDs.Count() == 0) originSongsIDs = new List<String> { "282942", "393058", "117445", "215816", "311338", "365408", "282905", "319864", "137896", "290680", "188808" };

            //originSongsIDs = new List<String> { "283068" };//, "335370", "347471", "373245", "347062", "347383", "304261", "277080", "340407", "354623", "362925", "314317", "323577", "301227", "324687" };

            //Create the Origin Points collection
            SongEndPointCollection originSongs = new SongEndPointCollection();

            toolBox.status = "Searching for Songs from Origin Songs";
            //Add an endpoint for each selected originsong
            foreach (String songID in originSongsIDs)
            {
                SongEndPoint songEndPoint = new SongEndPoint { songID = songID };
                originSongs.endPoints.Add(songID, songEndPoint);
            }

            Console.WriteLine("Top 50 Done: " + timer.ElapsedMilliseconds);


            int percentDoneCalc = 0;

            //Prepare the starting endpoints for the above selected songs and tie them to the origin collection, ignoring the player itself.
            foreach (Top10kPlayer player in players.top10kPlayers.Where(player => player.id != activePlayer.id && player.rank >= playerRankFrom && player.rank <= playerRankTo))
            {
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
                percentDoneCalc++;
                songSuggestCompletion = (0.0 + (4.0 * percentDoneCalc / (playerRankTo - playerRankFrom))) / 6.0;
            }
            Console.WriteLine("Completion: " + (songSuggestCompletion * 100) + "%");
            Console.WriteLine("Origin Endpoint Done: " + timer.ElapsedMilliseconds);

            //Create the suggested songs Endpoints
            toolBox.status = "Sorting Found Songs";
            SongEndPointCollection suggestedSongs = new SongEndPointCollection();

            percentDoneCalc = 0;
            //loop all origin songs
            foreach (SongEndPoint songEndPoint in originSongs.endPoints.Values)
            {
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
                percentDoneCalc++;
                songSuggestCompletion = (4.0 + (1.5 * percentDoneCalc / originSongs.endPoints.Values.Count())) / 6.0;
            }
            Console.WriteLine("Completion: " + (songSuggestCompletion * 100) + "%");
            Console.WriteLine("Suggest Endpoint Done: " + timer.ElapsedMilliseconds);

            //Calculate the scores on the songs for suggestions
            toolBox.status = "Evaluating Found Songs";
            suggestedSongs.SetRelevance(this, originSongs.endPoints.Count(), settings.requiredMatches);
            Console.WriteLine("Completion: " + (songSuggestCompletion * 100) + "%");
            Console.WriteLine("Score Relevance Calculations Done: " + timer.ElapsedMilliseconds);

            //Find most relevant songs for playlist selection
            toolBox.status = "Selecting Best Matching Songs";
            List<SongEndPoint> candidates = suggestedSongs.endPoints.Values.OrderByDescending(s => s.weightedRelevanceScore).ToList();

            candidates = candidates.GetRange(0, Math.Min(50, candidates.Count()));
            Console.WriteLine("Candidate Sorting Done: " + timer.ElapsedMilliseconds);

            //Make Playlist
            toolBox.status = "Making Playlist";
            Playlist playlist = new Playlist(settings.playlistSettings) { toolBox = toolBox };
            playlist.AddSongs(candidates.Select(c => c.songID).ToList());
            playlist.Generate();

            Console.WriteLine("Playlist Generation Done: " + timer.ElapsedMilliseconds);

            timer.Stop();
            Console.WriteLine("Time Spent: " + timer.ElapsedMilliseconds);

            songSuggestIDs = playlist.GetSongs();
            toolBox.status = "Ready";
        }


    }
}