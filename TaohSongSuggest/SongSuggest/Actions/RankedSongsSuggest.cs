using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using PlaylistNS;
using System.Diagnostics;
using LinkedData;
using Settings;
using SongSuggestNS;

namespace Actions
{
    public class RankedSongsSuggest
    {
        //Output Active?
        Boolean showDetailedOutout = false;

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

        //List for songs selected as Origin.
        List<String> originSongIDs;

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
        double betterAccCap = 1.1;
        double worseAccCap = 0.7;
        //int     songCount;     //Amount of songs to use before filtering to 50

        //Amount of Players the user got linked to. Low count then we remove betterAccCap limits.
        //Still low count, then the suggestions may be strange (Way too hard songs), we make sure to evaluate songs
        //(compare the songs min and max range to the players max PP on any song, and remove
        //unrealistic suggestions)
        int minPlayerLinks = 1000;
        int linkedPlayers = 0;

        //Value for how many spots must be expected to be improved before being shown in suggestions (unplayed songs are always shown)
        int improveSpots = 5;

        //Filter Results
        public List<String> distanceFilterOrdered;
        public List<String> styleFilterOrdered;
        public List<String> overWeightFilterOrdered;
        //in test        
        //PP for new distance
        List<String> ppFilterOrdered;
        //PP local vs Global
        List<String> ppLocalVSGlobalOrdered;

        //Creates a playlist with 50 suggested songs based on the link system.
        public void SuggestedSongs()
        {
            //TODO: When done splitting into groups, call groups from SongSuggest instead, and remove this Function.

            songSuggest.log?.WriteLine("Starting Song Suggest");

            //Sets the lower quality suggestions to false, different parts of the song evaluations can turn it true.
            songSuggest.lowQualitySuggestions = false;

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
            songSuggest.log?.WriteLine("Players Linked: {0}", linkedPlayers);
            songSuggest.log?.WriteLine("Playlist Generation Done: " + timer.ElapsedMilliseconds);

            timer.Stop();
            songSuggest.log?.WriteLine("Time Spent: " + timer.ElapsedMilliseconds);

            //Outputs results in the Console with how the different styles rankings
            ConsoleWriteStyleBreakdown();
        }

        public void Recalculate()
        {
            timer.Reset();
            timer.Start();
            songSuggest.log?.WriteLine("Starting Recalculations");
            EvaluateFilters();
            RemoveIgnoredSongs();
            CreatePlaylist();
            songSuggest.log?.WriteLine("Recalculations Done: " + timer.ElapsedMilliseconds);

            ////Outputs results in the Console with how the different styles rankings
            ConsoleWriteStyleBreakdown();
        }

        //Creates the needed linked data for song evaluation for the Active Player.
        //Until Active Players top 50 scores change *1 (replaced or better scores) no need to recalculate
        //*1 (Liked songs if active changes also counts as an update)
        public void CreateLinks()
        {
            //Updating scores has external wait time of the API call, so restarting measurement for the remainder of the update.
            songSuggest.log?.WriteLine("Starts the timer");
            timer.Start();

            //Get Link Data
            songSuggest.status = "Finding Link Data";
            top10kPlayers = songSuggest.top10kPlayers;
            songSuggest.log?.WriteLine("Done loading and generating the top10k player data: " + timer.ElapsedMilliseconds);

            //Find the Origin Song ID's based on Active Players data.
            songSuggest.status = "Finding Songs to Match";
            originSongIDs = OriginSongs(settings.useLikedSongs, settings.fillLikedSongs);

            //Create the Origin Points collection, and have them linked in Origin Points (Permabanned songs are not to be used).
            songSuggest.status = "Preparing Origin Songs";
            originSongs = CreateOriginPoints(originSongIDs, songSuggest.songBanning.GetPermaBannedIDs());
            //Check if there is no links and if, retry this time without removing lower acc links
            if (linkedPlayers < minPlayerLinks)
            {
                songSuggest.lowQualitySuggestions = true;
                songSuggest.log?.WriteLine("Not Enough Player Links Found ({0}) with Acc Limit on. Activate Limit Breaker.", linkedPlayers);
                betterAccCap = Double.MaxValue;
                originSongs = CreateOriginPoints(originSongIDs, songSuggest.songBanning.GetPermaBannedIDs());
            }

            songSuggest.log?.WriteLine("Completion: " + (songSuggestCompletion * 100) + "%");
            songSuggest.log?.WriteLine("Origin Endpoint Done: " + timer.ElapsedMilliseconds);

            //Link the Target end points in the Target End Point Collection
            targetSongs = CreateTargetPoints();
            songSuggest.log?.WriteLine("Completion: " + (songSuggestCompletion * 100) + "%");
            songSuggest.log?.WriteLine("Suggest Endpoint Done: " + timer.ElapsedMilliseconds);
        }

        //Order the songs via the different active filters
        public void CreateFilterRanks()
        {
            //Calculate the scores on the songs for suggestions
            songSuggest.status = "Evaluating Found Songs";
            EvaluateSongs();
            songSuggest.log?.WriteLine("Completion: " + (songSuggestCompletion * 100) + "%");
            songSuggest.log?.WriteLine("Score Relevance Calculations Done: " + timer.ElapsedMilliseconds);

            //Find most relevant songs for playlist selection
            songSuggest.status = "Selecting Best Matching Songs";

            //Filter on PP value compared to users songs PP values
            distanceFilterOrdered = targetSongs.endPoints.Values.OrderByDescending(s => s.weightedRelevanceScore).Select(p => p.songID).ToList();

            //Filter on how much over/under linked a song is in the active players data vs the global player population
            styleFilterOrdered = targetSongs.endPoints.Values.OrderBy(s => (0.0 + top10kPlayers.top10kSongMeta[s.songID].count) / (0.0 + s.proportionalStyle)).Select(p => p.songID).ToList();
            //Old bias for more suggestions from highly linked songs.
            //styleFilterOrdered = targetSongs.endPoints.Values.OrderBy(s => (0.0 + top10kPlayers.top10kSongMeta[s.songID].count) / (0.0 + s.songLinks.Count())).Select(p => p.songID).ToList();

            //Filter on how the selected songs rank are better than average
            overWeightFilterOrdered = targetSongs.endPoints.Values.OrderBy(s => s.averageRank).Select(p => p.songID).ToList();

            //***test
            //Filter on what the expected PP would be on a song.
            ppFilterOrdered = targetSongs.endPoints.Values.OrderByDescending(s => s.estimatedPP).Select(p => p.songID).ToList();
            //Filter on which PP is strongest in Local vs Global
            ppLocalVSGlobalOrdered = targetSongs.endPoints.Values.OrderByDescending(s => s.localVSGlobalPP).Select(p => p.songID).ToList();
        }

        //Creates a list of the origin songs (Liked and top 50)
        public List<String> OriginSongs(Boolean useLikedSongs, Boolean fillLikedSongs)
        {
            List<String> originSongsIDs = new List<String>();
            //Add Liked songs.
            songSuggest.log?.WriteLine("Use Liked Songs: " + useLikedSongs);

            if (useLikedSongs) originSongsIDs.AddRange(songSuggest.songLiking.GetLikedIDs());
            int targetCount = originSongsIDs.Count();


            songSuggest.log?.WriteLine("Songs in list: " + originSongsIDs.Count());

            //Add the standard origin songs if either normal mode of filler is activated
            if (!useLikedSongs || fillLikedSongs)
            {
                //***TST SWITCH BACK TO ACC***
                originSongsIDs.AddRange(BestPPandAccPlayedsongs(settings.extraSongs + 50));
                //originSongsIDs.AddRange(BestPPandAccPlayedsongs(settings.extraSongs + 50));
                //update targetsongs to either 50, or liked songs total, whichever is larger
                targetCount = Math.Max(50, targetCount);
            }

            originSongsIDs = originSongsIDs
                                .Distinct()         //Remove Duplicates
                                .Take(targetCount)  //Try and get 50 or all liked whichever is larger
                                .ToList();

            songSuggest.log?.WriteLine("Songs in list: " + originSongsIDs.Count());

            return originSongsIDs;
        }

        //get 50 most accurate songs of top %songCount% for linking. (or up to 50/%songCount% Percent of unbanned played ranked songs rounded up if less than 50 are available.)
        public List<String> BestPPandAccPlayedsongs(int songCount)
        {
            var filteredSongs = songSuggest.activePlayer.scores                                         //Look at players scores
                .Where(c => !songSuggest.songBanning.IsPermaBanned(c.Key, BanLike.BanType.SongSuggest)) //Remove Perma Banned Songs
                .OrderByDescending(c => c.Value.pp)                                                     //Order by pp
                .Skip(settings.skipSongsCount)                                                          //TEST CODE ... Ignore X songs to make list easier.
                .Take(songCount)                                                                        //Grab top songCount
                .OrderByDescending(c => c.Value.accuracy);                                              //Sort by acc

            int targetSongCount = (int)Math.Ceiling(50.0 / songCount * filteredSongs.Count());            //Finds originsongs to be either 50 or a percentage of 50/songCount !!!Potential BUG Rounding of doubles might give 51 instead of 50

            return filteredSongs.Take(targetSongCount)                                                  //Reduce the selections to target % or 50 (51)
                .OrderByDescending(c => c.Value.pp)                                                     //Reorder by PP so worst PP are removed if liked songs are used (or 51 songs found)
                .Select(c => c.Key)                                                                     //Get the songID
                .ToList();                                                                              //Put them in a list
        }

        //get 50 best rankedPercentile songs of top %songCount% for linking. (or up to 50/%songCount% Percent of unbanned played ranked songs rounded up if less than 50 are available.)
        public List<String> BestPPandRankedPercentilePlayedsongs(int songCount)
        {
            var filteredSongs = songSuggest.activePlayer.scores                                         //Look at players scores
                .Where(c => !songSuggest.songBanning.IsPermaBanned(c.Key, BanLike.BanType.SongSuggest)) //Remove Perma Banned Songs
                .OrderByDescending(c => c.Value.pp)                                                     //Order by pp
                .Take(songCount)                                                                        //Grab top songCount
                .OrderBy(c => c.Value.rankPercentile);                                                  //Sort by rank percentile

            int targetSongCount = (int)Math.Ceiling(50.0 / songCount * filteredSongs.Count());          //Finds originsongs to be either 50 or a percentage of 50/songCount !!!Potential BUG Rounding of doubles might give 51 instead of 50

            return filteredSongs.Take(targetSongCount)                                                  //Reduce the selections to target % or 50 (51)
                .OrderByDescending(c => c.Value.pp)                                                     //Reorder by PP so worst PP are removed if liked songs are used (or 51 songs found)
                .Select(c => c.Key)                                                                     //Get the songID
                .ToList();                                                                              //Put them in a list
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

            //Reset link count for new generation.
            linkedPlayers = 0;

            foreach (Top10kPlayer player in top10kPlayers.top10kPlayers.Where(player => player.id != songSuggest.activePlayer.id && player.rank >= settings.rankFrom && player.rank <= settings.rankTo))
            {
                //Loop all preselected origin songs on a player
                foreach (Top10kScore playerSong in player.top10kScore.Where(playerSong => originSongs.endPoints.ContainsKey(playerSong.songID)))
                {
                    //Only check if score is high enough if played actually played the song.
                    bool playedSong = songSuggest.activePlayer.scores.ContainsKey(playerSong.songID);
                    bool validSong = playedSong ? songSuggest.activePlayer.scores[playerSong.songID].pp * betterAccCap > playerSong.pp && songSuggest.activePlayer.scores[playerSong.songID].pp * worseAccCap < playerSong.pp : true;

                    //bool validSong = playedSong ? songSuggest.activePlayer.scores[playerSong.songID].pp * betterAccCap > playerSong.pp : true;


                    //Skip link if the targetsongs PP is too high compared to original players score
                    if (validSong) // && songSuggest.activePlayer.scores[playerSong.songID].pp / betterAccCap < playerSong.pp)
                    {
                        //Each player can be counted 50 times, as there is 50 songs to link from.
                        linkedPlayers++;
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

            //New test to try and guess PP
            targetSongs.SetPP(songSuggest);

            //New test to compare local group vs global group stuff
            targetSongs.SetLocalPP(songSuggest);

        }

        //Takes the orderes suggestions and apply the filter values to their ranks, and create the nameplate orderings
        public void EvaluateFilters()
        {
            Dictionary<String, double> totalScore = new Dictionary<String, double>();

            //Get Base Weights reset them from % value to [0-1], and must not all be 0)
            double modifierDistance = 0.0; //settings.filterSettings.modifierPP / 100;  //Deprecated so disabled in future code.
            double modifierStyle = settings.filterSettings.modifierStyle / 100;
            double modifierOverweight = settings.filterSettings.modifierOverweight / 100;
            //***test (hardcoded to max)
            double modifierPP = 0;
            double modifierPPLocalVSGlobal = 0;

            //reset if all = 0, reset to 100%.
            if (modifierStyle == 0 && modifierOverweight == 0) modifierStyle = modifierOverweight = 1.0;

            songSuggest.log?.WriteLine($"Style: {modifierStyle} Overweight: {modifierOverweight}");

            //Get count of candidates, and remove 1, as index start as 0, so max value is songs-1
            double totalCandidates = distanceFilterOrdered.Count() - 1;

            //As all 3 filters contain same ID's we can loop the song IDs from either of the filters, and calculate their combined score.
            foreach (String distanceCandidate in distanceFilterOrdered)
            {
                //Get the location of the candidate in the list as a [0-1] value
                double distanceValue = distanceFilterOrdered.IndexOf(distanceCandidate) / totalCandidates;
                double styleValue = styleFilterOrdered.IndexOf(distanceCandidate) / totalCandidates;
                double overWeightedValue = overWeightFilterOrdered.IndexOf(distanceCandidate) / totalCandidates;
                //***test
                //ppValue for distance replacement
                double ppValue = ppFilterOrdered.IndexOf(distanceCandidate) / totalCandidates;
                //ppLocalvsGlobal
                double ppLocalVSGlobalValue = ppLocalVSGlobalOrdered.IndexOf(distanceCandidate) / totalCandidates;

                //Switch the range from [0-1] to [0.5-1.5] and reduce the gap based on modifier weight.
                //**Spacing between values may be more correct to consider a log spacing (e.g. due to 1.5*.0.5 != 1)
                //**But as values are kept around 1, and it is not important to keep total average at 1, the difference in
                //**Actual ratings in the 0.5 to 1.5 range is minimal at the "best suggestions range" even with quite a few filters.
                //**So a "correct range" of 0.5 to 2 would give a higher penalty on bad matches on a single filter, so current
                //**setup means a song must do worse on more filters to actual lose rank, which actually may be prefered.
                double distanceTotal = distanceValue * modifierDistance + (1.0 - 0.5 * modifierDistance);
                double styleTotal = styleValue * modifierStyle + (1.0 - 0.5 * modifierStyle);
                double overWeightedTotal = overWeightedValue * modifierOverweight + (1.0 - 0.5 * modifierOverweight);
                double ppTotal = ppValue * modifierPP + (1.0 - 0.5 * modifierPP);
                double ppLocalVSGlobalTotal = ppLocalVSGlobalValue * modifierPPLocalVSGlobal + (1.0 - 0.5 * modifierPPLocalVSGlobal);

                //Get the songs multiplied average 
                double score = distanceTotal * styleTotal * overWeightedTotal * ppTotal * ppLocalVSGlobalTotal;

                //Add song ID and its score to a list for sorting and reducing size for the playlist generation
                totalScore.Add(distanceCandidate, score);
            }

            //Sort list, and get song ID's only
            sortedSuggestions = totalScore.OrderBy(s => s.Value).Select(s => s.Key).ToList();

            //The suggestions may be weak if there is a low amount of Links, so current suggestions needs evaluation to make
            //sure if link count is low that potential too hard songs are removed.
            //readd all remaining songs to the ends of the list from easiest to hardest (if they are on enough top 20's), as this makes
            //it possible to filter disliked, too hard songs etc normally, and always provide a list of 50 songs.
            LowLinkEvaluation();
        }

        //There is not enough links to have a high confidence in all results are doable
        //So removes any songs outside expected range in min/max PP values
        //Then takes all remaining songs with at least a few plays and readd them after actual suggestions to make sure player
        //Can ban/have recently played songs removed without dropping under 50 suggestions.
        public void LowLinkEvaluation()
        {
            //Skip this if enough links. (It is possible that removing the low accuracy filter ended up giving enough links that song
            //suggestions are good, even if the players acc is so low that the Better Acc filter was triggered).
            if (linkedPlayers < minPlayerLinks)
            {
                songSuggest.log?.WriteLine("Low Linking found");
                //Enable the warning for additonal steps to ensure enough songs.
                songSuggest.lowQualitySuggestions = true;
                //Get the players max PP
                //Find all pp scores of the active player, and if none are found set max score to 0 (new player)
                List<float> allPlayerPPScores = songSuggest.activePlayer.scores.OrderByDescending(c => c.Value.pp).Take(1).Select(c => c.Value.pp).ToList();
                //Get largst PP score, or set to 0 if none achieved.                
                double playerMaxPP = allPlayerPPScores.Count > 0 ? allPlayerPPScores[0] : 0;
                songSuggest.log?.WriteLine("PP:" + playerMaxPP);
                songSuggest.log?.WriteLine("Filtering out songs that are expected too hard");
                songSuggest.log?.WriteLine("Songs before filtering: {0}", sortedSuggestions.Count());
                //Remove songs that have too high a min PP (expected song is outside the players skill)                
                //Remove songs that have too high a max PP (expected players Acc is lacking)
                //Remove songs without 3 plays (The songs scores could be random values, so rather remove them for now)

                sortedSuggestions = sortedSuggestions
                    .Where(c => songSuggest.top10kPlayers.top10kSongMeta[c].minScore < 1.2 * playerMaxPP
                    && songSuggest.top10kPlayers.top10kSongMeta[c].maxScore < 1.5 * playerMaxPP
                    && songSuggest.top10kPlayers.top10kSongMeta[c].count >= 3)
                    .ToList();

                songSuggest.log?.WriteLine("Songs left after filtering: {0}", sortedSuggestions.Count());

                //Find all songs with at least 3 plays, and sort them by MaxPP scores, so easiest is first, and remove already approved songs
                List<String> remainingSongs = songSuggest.top10kPlayers.top10kSongMeta
                    .Where(c => c.Value.count >= 3)
                    .OrderBy(c => c.Value.maxScore)
                    .Select(c => c.Value.songID)
                    .Except(sortedSuggestions)
                    .ToList();

                //Add the songs not already suggested to the list.
                sortedSuggestions.AddRange(remainingSongs);
            }
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
            songSuggestIDs = filteredSuggestions.Take(settings.playlistLength).ToList();//filteredSuggestions.GetRange(0, Math.Min(settings.playlistLength, filteredSuggestions.Count()));

            PlaylistManager playlist = new PlaylistManager(settings.playlistSettings) { songSuggest = songSuggest };
            playlist.AddSongs(songSuggestIDs);
            playlist.Generate();
        }

        public void ConsoleWriteStyleBreakdown()
        {
            if (showDetailedOutout)
            {
                int rank = 0;
                foreach (string songID in sortedSuggestions)
                {
                    rank++;
                    List<String> playerRankedPP = songSuggest.activePlayer.scores.Values.OrderByDescending(p => p.pp).ToList().Select(p => p.songID).ToList();

                    int actualPlayerRank = playerRankedPP.IndexOf(songID) + 1;
                    String actualPlayerRankTxt = actualPlayerRank == 0 ? "-" : "" + actualPlayerRank;

                    int ppRank = distanceFilterOrdered.IndexOf(songID) + 1;
                    int styleRank = styleFilterOrdered.IndexOf(songID) + 1;
                    int owRank = overWeightFilterOrdered.IndexOf(songID) + 1;

                    String songName = songSuggest.songLibrary.GetName(songID);
                    String songDifc = songSuggest.songLibrary.GetDifficultyName(songID);

                    String songInfo = songName + " (" + songDifc + " - " + songID + ")";

                    double globalPP = songSuggest.top10kPlayers.top10kSongMeta[songID].maxScore;

                    ////***test PP -> Distance
                    double playerPP = songSuggest.activePlayer.GetScore(songID);
                    double estimatedPP = targetSongs.endPoints.ContainsKey(songID) ? targetSongs.endPoints[songID].estimatedPP : 0;
                    double gainablePP = estimatedPP - playerPP;

                    //***Test PP local vs global
                    double localVSGlobalPP = targetSongs.endPoints.ContainsKey(songID) ? targetSongs.endPoints[songID].localVSGlobalPP : 0;

                    //songSuggest.log?.WriteLine("#:{0}\tPPdiff:{8}\testPP:{9}\tactPP:{10}\tAc:{1}\tPP:{2}\tSt:{3}\tOw:{4}\t: {5} ({6} - {7})", rank, actualPlayerRankTxt, ppRank, styleRank, owRank, songName, songDifc, songID, gainablePP,estimatedPP,playerPP);

                    //songSuggest.log?.WriteLine("#:{0}\t{2}\tRatio:{3}\t{1}", rank, songInfo, actualPlayerRankTxt, localVSGlobalPP);
                    songSuggest.log?.WriteLine("#:{0}\tDistance:{3}\tStyle :{4}\tOW:{5}\tActual:{2}\t{1}", rank, songInfo, actualPlayerRankTxt, ppRank, styleRank, owRank);
                }

                //    rank = 0;
                //    foreach (String songID in originSongIDs)
                //    {
                //        rank++;

                //        String songName = songSuggest.songLibrary.GetName(songID);
                //        String songDifc = songSuggest.songLibrary.GetDifficultyName(songID);

                //        String songInfo = songName + " (" + songDifc + " - " + songID + ")";

                //        songSuggest.log?.WriteLine("#:{0,3}   {1}",rank,songInfo);

                //    }

            }
        }
    }

    //A filter contains a way to evaluate rankings and provide both an ordering of songs linked, as well as the strength (default methode can
    //be overwritten if special cases needs non standard spreads, such as Filters that adds specific bonusses or penalties in a limited scope.
    //e.g. Recently played penalty, less played songs bonus
    public abstract class Filter
    {
        //Default the filter to inactive and 0 strength until set.
        private Boolean active = false;
        private Boolean calculated = false;
        public Double filterStrength { get; set; } = 0;

        //Name of the Filter
        public abstract string Name();
        //Description of the Filter
        public abstract string Description();

        //SongID for lookup and relevant data for the song
        private SortedDictionary<String, FilterSongData> songData;
        private SongEndPointCollection originSongs;
        private SongEndPointCollection targetSongs;

        //Creates a new filter and sets the endpoint collections for calculations
        public Filter(SongEndPointCollection originSongs, SongEndPointCollection targetSongs)
        {
            this.originSongs = originSongs;
            this.targetSongs = targetSongs;
        }

        //Sets the strength of the filter as a value between 0 and 1 (0 sets the filter inactive)
        public void SetFilterStrength(double strength)
        {
            if (strength == 0)
            {
                filterStrength = 0;
                active = false;
            }
            else
            {
                filterStrength = strength;
                active = true;
            }
        }

        public Boolean IsActive()
        {
            return active;
        }

        //Make the pre calculations for the filter so order can be made.
        public abstract void Calculate();

        //A songs default multiplier as a value based on filter strength averaged around 1. Some filters may use other caluclations.
        public double SongMultiplier(String songID)
        {
            //Returns 1 if the filter is inactive, or if it has not been calculated yet
            if (!active) return 1;
            if (!calculated) return 1;

            //Since we start with 0 index, the max song is 1 less than the elements.
            double maxCount = songData.Count() - 1;
            //For the calculations we use the 0 index rank, so we get a 0 to 1 spread.
            double songRank = songData[songID].songRank - 1;
            //A value from 0 to 1
            double unfilteredStrength = songRank / maxCount;
            //Reduce the spread with the filterstrength.
            double filteredStrength = unfilteredStrength * filterStrength;
            //Move the found strength up to center around 1 (Lowest value is half a filterStrength lower, and max is half a filterStrength higher)
            double strength = filteredStrength + 1 - (filterStrength / 2);
            return strength;
        }
    }

    //Contains song specific data for a filter to store, along with a default Filter Values per song basis.
    public abstract class FilterSongData
    {
        public String songID;
        //Rank of song starting with 1 for best
        public int songRank;

    }
    public class FilterManager
    {

    }
}