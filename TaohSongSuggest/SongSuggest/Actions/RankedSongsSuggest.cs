using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using PlaylistNS;
using ActivePlayerData;
using System.Diagnostics;
using LinkedData;
using Settings;
using SongSuggestNS;

namespace Actions
{
    public class RankedSongsSuggest
    {
        public SongSuggest songSuggest { get; set; }
        //Information for the GUI on how far the update progress is.
        public double songSuggestCompletion { get; set; }

        //All found songs ordered by current filter settings.
        public List<String> sortedSuggestions { get; set; }
        
        //Found songs with ignored songs removed
        public List<String> filteredSuggestions { get; set; }
        //ID's of songs used in actual playlist (50 songs)
        public List<String> songSuggestIDs { get; set; }

        //Settings file for current active session of RankedSongsSuggest, can be replaced/updated for refiltering.
        public SongSuggestSettings settings { get; set; }

        //private utility classes, consider direct linking them in future rework.
        Top10kPlayers top10kPlayers;

        //Debug Timer, used to see if calculations end up being too slow.
        Stopwatch timer = new Stopwatch();

        //The two collections of endpoints of the Origin and Target songs from 10k player data based on Active Players data.
        //Active Player Songs = Top 50 ranked and or liked songs
        //Origin = (10k player) Songs that match Active Players Songs
        //Target = Suggested Songs linked from Origin (within a single 10k player, 1 origin -> 19 target links of other top 20 songs.)
        //Only needs recalculation if Originsongs change, or the player improves a top 50 score (less filtered betterAccCap suggestions).
        SongEndPointCollection originSongs;
        SongEndPointCollection targetSongs;

        //Set how many times more pp a person may have be performed on a song before their songs are ignored.
        //1.2 = 120%, 1.1 = 110% etc.
        //1.2 seems to be a good value to cut off low/high acc linkage, while still allowing a player room for growth suggestions
        //This may be a usefull Difficulty setting in the future.
        readonly double betterAccCap = 1.2;

        //Value for how many spots must be expected to be improved before being shown in suggestions (unplayed songs are always shown)
        int improveSpots = 5;


        //Filter Results
        List<String> ppFilterOrdered;
        List<String> styleFilterOrdered;
        List<String> overWeightFilterOrdered;

        //Creates a playlist with 50 suggested songs based on the link system.
        public void SuggestedSongs()
        {
            //TODO: When done splitting into groups, call groups from SongSuggest instead, and remove this Function.

            Console.WriteLine("Starting Song Suggest");

            //Setup Base Linking (song links).
            CreateLinks();

            //Generate the different filters rankings. (Calculate Scores, and Rank them)
            CreateFilterRanks();

            //Takes the orderes lists runs through them and assign points based on order.
            EvaluateFilters();

            //Removes filtered songs (Played/Played within X days/Banned/Not expected improveable atm) depending on settings
            RemoveIgnoredSongs();

            //Creates the playist of remaining songs
            CreatePlaylist();

            //----- Console Writeline for Debug -----
            Console.WriteLine("Playlist Generation Done: " + timer.ElapsedMilliseconds);

            timer.Stop();
            Console.WriteLine("Time Spent: " + timer.ElapsedMilliseconds);

            ////Outputs results in the Console with how the different styles rankings
            //ConsoleWriteStyleBreakdown();
        }

        public void Recalculate()
        {
            timer.Reset();
            timer.Start();
            Console.WriteLine("Starting Recalculations");
            EvaluateFilters();
            RemoveIgnoredSongs();
            CreatePlaylist();
            Console.WriteLine("Recalculations Done: " + timer.ElapsedMilliseconds);
        }

        //Creates the needed linked data for song evaluation for the Active Player.
        //Until Active Players top 50 scores change *1 (replaced or better scores) no need to recalculate
        //*1 (Liked songs if active changes also counts as an update)
        public void CreateLinks()
        {
            //Updating scores has external wait time of the API call, so restarting measurement for the remainder of the update.
            Console.WriteLine("Starts the timer");
            timer.Start();

            //Get Link Data
            songSuggest.status = "Finding Link Data";
            top10kPlayers = songSuggest.top10kPlayers;
            Console.WriteLine("Done loading and generating the top10k player data: " + timer.ElapsedMilliseconds);

            //Find the Origin Song ID's based on Active Players data.
            songSuggest.status = "Finding Songs to Match";
            List<String> originSongIDs = OriginSongs(settings.useLikedSongs, settings.fillLikedSongs);

            //Create the Origin Points collection, and have them linked in Origin Points (Permabanned songs are not to be used).
            songSuggest.status = "Preparing Origin Songs";
            originSongs = CreateOriginPoints(originSongIDs, songSuggest.songBanning.GetPermaBannedIDs());
            Console.WriteLine("Completion: " + (songSuggestCompletion * 100) + "%");
            Console.WriteLine("Origin Endpoint Done: " + timer.ElapsedMilliseconds);

            //Link the Target end points in the Target End Point Collection
            targetSongs = CreateTargetPoints();
            Console.WriteLine("Completion: " + (songSuggestCompletion * 100) + "%");
            Console.WriteLine("Suggest Endpoint Done: " + timer.ElapsedMilliseconds);


        }

        //Order the songs via the different active filters
        public void CreateFilterRanks()
        {
            //Calculate the scores on the songs for suggestions
            songSuggest.status = "Evaluating Found Songs";
            EvaluateSongs();
            Console.WriteLine("Completion: " + (songSuggestCompletion * 100) + "%");
            Console.WriteLine("Score Relevance Calculations Done: " + timer.ElapsedMilliseconds);

            //Find most relevant songs for playlist selection
            songSuggest.status = "Selecting Best Matching Songs";

            //Filter on PP value compared to users songs PP values
            ppFilterOrdered = targetSongs.endPoints.Values.OrderByDescending(s => s.weightedRelevanceScore).Select(p => p.songID).ToList();
            
            //Filter on how much over/under linked a song is in the active players data vs the global player population
            styleFilterOrdered = targetSongs.endPoints.Values.OrderBy(s => (0.0 + top10kPlayers.top10kSongMeta[s.songID].count) / (0.0 + s.proportionalStyle)).Select(p => p.songID).ToList();
            //Old bias for more suggestions from highly linked songs.
            //styleFilterOrdered = targetSongs.endPoints.Values.OrderBy(s => (0.0 + top10kPlayers.top10kSongMeta[s.songID].count) / (0.0 + s.songLinks.Count())).Select(p => p.songID).ToList();

            //Filter on how the selected songs rank are better than average
            overWeightFilterOrdered = targetSongs.endPoints.Values.OrderBy(s => s.averageRank).Select(p => p.songID).ToList();
        }

        //Creates a list of the origin songs (Liked and top 50
        public List<String> OriginSongs(Boolean useLikedSongs, Boolean fillLikedSongs)
        {
            List<String> originSongsIDs = new List<String>();
            //Add Liked songs.
            Console.WriteLine("Use Liked Songs: " + useLikedSongs);

            if (useLikedSongs) originSongsIDs = originSongsIDs.Union(songSuggest.songLiking.GetLikedIDs()).ToList();

            Console.WriteLine("Songs in list: " + originSongsIDs.Count());

            //Fill list to 50 if liked songs are not used, or the selection is made to fill
            if (!useLikedSongs || fillLikedSongs) originSongsIDs = originSongsIDs.Union(songSuggest.activePlayer.GetTop(50 - originSongsIDs.Count())).ToList();

            Console.WriteLine("Songs in list: " + originSongsIDs.Count());

            //If no songs are added (user has 0 ranked plays) a "random" list is generated, selected top 10k players and random songs from their lists.
            if (originSongsIDs.Count() == 0)
            {
                Console.WriteLine("No Songs found, lets make something up!");
                originSongsIDs = new List<String> { "282942", "393058", "117445", "215816", "311338", "365408", "282905", "319864", "137896", "290680", "188808" };
            }

            return originSongsIDs;
        }

        //Sets the Origin Endpoint collection up, and links all the SongLinks to the Origin points
        public SongEndPointCollection CreateOriginPoints(List<String> originSongIDs, List<String> ignoreSongs)
        {
            originSongs = new SongEndPointCollection();

            songSuggest.status = "Searching for Songs from Origin Songs";
            int percentDoneCalc = 0;
            //Add an endpoint for each selected originsong
            foreach (String songID in originSongIDs)
            {
                SongEndPoint songEndPoint = new SongEndPoint { songID = songID };
                originSongs.endPoints.Add(songID, songEndPoint);
            }

            //Prepare the starting endpoints for the above selected songs and tie them to the origin collection, ignoring the player itself.
            foreach (Top10kPlayer player in top10kPlayers.top10kPlayers.Where(player => player.id != songSuggest.activePlayer.id && player.rank >= settings.rankFrom && player.rank <= settings.rankTo))
            {
                //Loop all preselected origin songs on a player
                foreach (Top10kScore playerSong in player.top10kScore.Where(playerSong => originSongs.endPoints.ContainsKey(playerSong.songID)))
                {
                    //Only check if score is high enough if played actually played the song.
                    bool playedSong = songSuggest.activePlayer.scores.ContainsKey(playerSong.songID);
                    bool validSong = playedSong ? songSuggest.activePlayer.scores[playerSong.songID].pp * betterAccCap > playerSong.pp : true;

                    //Skip link if the targetsongs PP is too high compared to original players score
                    if (validSong) // && songSuggest.activePlayer.scores[playerSong.songID].pp / betterAccCap < playerSong.pp)
                    {
                        //Loop songs again for endpoints, skipping linking itself, as well as ignoreSongs
                        foreach (Top10kScore suggestedSong in player.top10kScore.Where(suggestedSong => suggestedSong.rank != playerSong.rank && !ignoreSongs.Contains(suggestedSong.songID)))
                        {
                            SongLink songLink = new SongLink
                            {
                                playerID = player.id,
                                originSongScore = playerSong,
                                targetSongScore = suggestedSong
                            };
                            originSongs.endPoints[playerSong.songID].songLinks.Add(songLink);
                        }
                    }
                }
                percentDoneCalc++;
                songSuggestCompletion = (0.0 + (4.0 * percentDoneCalc / (settings.rankTo - settings.rankFrom))) / 6.0;
            }

            //Create the suggested songs Endpoints
            songSuggest.status = "Sorting Found Songs";
            targetSongs = new SongEndPointCollection();
            return originSongs;
        }

        //Sets the Target Endpoint collection up, and links the origin songs to their respective target endpoint
        public SongEndPointCollection CreateTargetPoints()
        {
            SongEndPointCollection targetSongs = new SongEndPointCollection();
            //Creates a new target end point collection to work with
            int percentDoneCalc = 0;
            //loop all origin songs
            foreach (SongEndPoint songEndPoint in originSongs.endPoints.Values)
            {
                //loop all links in that active origin song
                foreach (SongLink songLink in songEndPoint.songLinks)
                {
                    //If song is not present, make an endpoint for it
                    if (!targetSongs.endPoints.ContainsKey(songLink.targetSongScore.songID))
                    {
                        SongEndPoint suggestedSongEndPoint = new SongEndPoint { songID = songLink.targetSongScore.songID };
                        targetSongs.endPoints.Add(songLink.targetSongScore.songID, suggestedSongEndPoint);
                    }

                    //add endpoint to suggested song
                    targetSongs.endPoints[songLink.targetSongScore.songID].songLinks.Add(songLink);
                }
                percentDoneCalc++;
                songSuggestCompletion = (4.0 + (1.5 * percentDoneCalc / originSongs.endPoints.Values.Count())) / 6.0;
            }
            return targetSongs;
        }

        //Generate the weighting for the different Filters and stores them in the Endpoint Data.
        public void EvaluateSongs()
        {
            //TODO: Should be split into new Distance calculation, and overWeight calculculation, and update the variables needed to be sent.

            //Calculate strength for filter rankings in the SongLink data with needed data sent along.
            targetSongs.SetRelevance(this, originSongs.endPoints.Count(), settings.requiredMatches);
            targetSongs.SetStyle(originSongs);

            //targetSongs.SetDistance(songSuggest); //Needs reworking.
        }

        //Takes the orderes suggestions and apply the filter values to their ranks, and create the nameplate orderings
        public void EvaluateFilters()
        {
            Dictionary<String, double> totalScore = new Dictionary<String, double>();

            //Get Base Weights reset them from % value to [0-1], and must not all be 0)
            double modifierPP = settings.filterSettings.modifierPP / 100;
            double modifierStyle = settings.filterSettings.modifierStyle / 100;
            double modifierOverweight = settings.filterSettings.modifierOverweight / 100;

            //reset if all = 0, reset to 100%.
            if (modifierPP == 0 && modifierStyle == 0 && modifierOverweight == 0) modifierPP = modifierStyle = modifierOverweight = 1.0;

            Console.WriteLine("PP: {0} Style: {1} Overweight: {2}", modifierPP, modifierStyle, modifierOverweight);

            //Get count of candidates, and remove 1, as index start as 0, so max value is songs-1
            double totalCandidates = ppFilterOrdered.Count() - 1;

            //As all 3 filters contain same ID's we can loop the song IDs from either of the filters, and calculate their combined score.
            foreach (String ppCandidate in ppFilterOrdered)
            {
                //Get the location of the candidate in the list as a [0-1] value
                double ppValue = ppFilterOrdered.IndexOf(ppCandidate) / totalCandidates;
                double styleValue = styleFilterOrdered.IndexOf(ppCandidate) / totalCandidates;
                double overWeightedValue = overWeightFilterOrdered.IndexOf(ppCandidate) / totalCandidates;

                //Switch the range from [0-1] to [0.5-1.5] and reduce the gap based on modifier weight.
                //**Spacing between values may be more correct to consider a log spacing (e.g. due to 1.5*.0.5 != 1)
                //**But as values are kept around 1, and it is not important to keep total average at 1, the difference in
                //**Actual ratings in the 0.5 to 1.5 range is minimal at the "best suggestions range" even with quite a few filters.
                //**So a "correct range" of 0.5 to 2 would give a higher penalty on bad matches on a single filter, so current
                //**setup means a song must do worse on more filters to actual lose rank, which actually may be prefered.
                double ppTotal = ppValue * modifierPP + (1.0 - 0.5 * modifierPP);
                double styleTotal = styleValue * modifierStyle + (1.0 - 0.5 * modifierStyle);
                double overWeightedTotal = overWeightedValue * modifierOverweight + (1.0 - 0.5 * modifierOverweight);

                //Get the songs multiplied average 
                double score = ppTotal * styleTotal * overWeightedTotal;

                //Add song ID and its score to a list for sorting and reducing size for the playlist generation
                totalScore.Add(ppCandidate, score);
            }

            //Sort list, and get song ID's only
            sortedSuggestions = totalScore.OrderBy(s => s.Value).Select(s => s.Key).ToList();
        }

        //Filters out any songs that should not be in the generated playlist
        //Ignore All Played
        //Ignore X Days
        //Banned Songs
        //Songs that is not expected improveable
        public void RemoveIgnoredSongs()
        {
            //Filter out ignoreSongs before making the playlist.
            //Get the ignore lists ready (permaban, ban, and improved within X days, not improveable by X ranks)
            songSuggest.status = "Preparing Ignore List";
            List<String> ignoreSongs = CreateIgnoreLists(settings.ignorePlayedAll ? -1 : settings.ignorePlayedDays);
            filteredSuggestions = sortedSuggestions.Where(s => !ignoreSongs.Contains(s)).ToList();
        }

        //Create a List of songID's to filter out. Consider splitting it so Permaban does not get links, while
        //standard temporary banned, and recently played gets removed after.
        //Send -1 if all played should be ignored, else amount of days to ignore.
        public List<String> CreateIgnoreLists(int ignoreDays)
        {
            List<String> ignoreSongs = new List<String>();

            //Ignore recently/all played songs
            //Add either all played songs
            if (ignoreDays == -1)
            {
                ignoreSongs.AddRange(songSuggest.activePlayer.scores.Keys);
            }
            //Or the songs only played within a given time periode
            else
            {
                ignoreSongs.AddRange(songSuggest.activePlayer.GetYoungerThan(ignoreDays));
            }

            //Add the banned songs to the ignoresong list if not already on it.
            ignoreSongs = ignoreSongs.Union(songSuggest.songBanning.GetBannedIDs()).ToList();

            //Add songs that is not expected to be improveable by X ranks
            if (settings.ignoreNonImproveable)
            {
                List<String> activePlayersPPSortedSongs = songSuggest.activePlayer.scores.Values.OrderByDescending(p => p.pp).ToList().Select(p => p.songID).ToList();

                int suggestedSongRank = 0;
                foreach (string songID in sortedSuggestions)
                {
                    int currentSongRank = activePlayersPPSortedSongs.IndexOf(songID);
                    //Add songs ID to ignore list if current rank is not expected improveable by at least X spots, and it is not an unplayed song
                    if (currentSongRank < suggestedSongRank + improveSpots && currentSongRank != -1)
                    {
                        ignoreSongs.Add(songID);
                    }
                    suggestedSongRank++;
                }
            }
            return ignoreSongs;
        }

        //Make Playlist
        public void CreatePlaylist()
        {
            songSuggest.status = "Making Playlist";

            //Select 50 best suggestions
            songSuggestIDs = filteredSuggestions.GetRange(0, Math.Min(50, filteredSuggestions.Count()));

            Playlist playlist = new Playlist(settings.playlistSettings) { songSuggest = songSuggest };
            playlist.AddSongs(songSuggestIDs);
            playlist.Generate();
        }

        public void ConsoleWriteStyleBreakdown()
        {
            int rank = 0;
            foreach (string songID in sortedSuggestions)
            {
                rank++;
                List<String> playerRankedPP = songSuggest.activePlayer.scores.Values.OrderByDescending(p => p.pp).ToList().Select(p => p.songID).ToList();

                int actualPlayerRank = playerRankedPP.IndexOf(songID) + 1;
                String actualPlayerRankTxt = actualPlayerRank == 0 ? "-" : "" + actualPlayerRank;

                int ppRank = ppFilterOrdered.IndexOf(songID) + 1;
                int styleRank = styleFilterOrdered.IndexOf(songID) + 1;
                int owRank = overWeightFilterOrdered.IndexOf(songID) + 1;

                String songName = songSuggest.songLibrary.GetName(songID);
                String songDifc = songSuggest.songLibrary.GetDifficultyName(songID);

                Console.WriteLine("#:{0}\tAc:{1}\tPP:{2}\tSt:{3}\tOw:{4}\t: {5} ({6} - {7})", rank, actualPlayerRankTxt, ppRank, styleRank, owRank, songName, songDifc, songID);
            }
        }
    }
}