using PlaylistNS;
using SongSuggestNS;
using Settings;
using System.Linq;
using System.Collections.Generic;
using System;
using BanLike;
using ActivePlayerData;
using System.Data;

namespace Actions
{
    //Creates a playlist with the 100 oldest maps for a player.
    public class OldestSongs
    {
        public SongSuggest songSuggest { get; set; }
        public PlaylistManager playlist;
        private OldestSongSettings settings; //Temporary settings storage for the sorting.

        public OldestSongs(SongSuggest songSuggest)
        {
            this.songSuggest = songSuggest;
        }

        public void GeneratePlaylist(OldestSongSettings settings)
        {
            this.settings = settings;
            
            //Create empty playlist, and reset output window.
            playlist = new PlaylistManager(settings.playlistSettings) {songSuggest = songSuggest};
           
            //Add up to 100 oldest song to playlist that has not been banned, and is within given parameters.
            songSuggest.status = $"Finding {settings.playlistLength} {settings.songSelection}";

            //Get a list of target songs
            var selectedSongs = SelectSongs();

            //Sort list by selection order
            selectedSongs = SortSongs(selectedSongs, settings.songSelection);

            //Reverse ordering if before selection if needed.
            if (settings.reverseSelectionOrdering) selectedSongs.Reverse();

            //reduce list to the wanted amount of songs.
            selectedSongs = selectedSongs.Take(settings.playlistLength).ToList();

            //Order the selected songs
            selectedSongs = SortSongs(selectedSongs, settings.songOrdering);

            //Reverse display ordering in playlist if needed.
            if (settings.reversePlaylistOrdering) selectedSongs.Reverse();

            playlist.AddSongs(selectedSongs);

            //Generate and save a playlist with the selected songs in the playlist.
            songSuggest.status = "Generating Playlist";
            playlist.Generate();
        }

        //Gets a ID's of either all songs, unplayed, or played only, and remove any banned songs.
        private List<String> SelectSongs()
        {
            //Get unplayed
            List<String> unplayedSongs = new List<String>();

            unplayedSongs = songSuggest.songLibrary.GetAllRankedSongIDs(settings.unplayedSongCategories).Except(songSuggest.activePlayer.scores.Values.Select(c=> c.songID)).ToList();

            //Get available songs within settings requirements
            List<String> playedSongs = songSuggest.activePlayer.scores.Values                   //Get Players songs
                .Where(c => c.accuracy < settings.ignoreAccuracyEqualAbove)                     //Ignore high Acc
                .Where(c => c.timeSet < DateTime.UtcNow.AddDays(-settings.ignorePlayedDays))    //Ignore newest plays
                .Select(c => c.songID)
                .ToList();

            playedSongs = songSuggest.songLibrary.GetAllRankedSongIDs(settings.playedSongCategories, playedSongs);

            List<String> brokenSongs = songSuggest.songLibrary.GetAllRankedSongIDs(SongCategory.BrokenDownloads);

            //Remove broken songs if they are not selected.
            if ((settings.playedSongCategories & SongCategory.BrokenDownloads) == 0)
            {
                playedSongs = playedSongs.Except(brokenSongs).ToList();
            }
            if ((settings.unplayedSongCategories & SongCategory.BrokenDownloads) == 0 )
            {
                unplayedSongs = unplayedSongs.Except(brokenSongs).ToList();
            }


            //Combine Unplayed and Oldest Lists, and remove banned songs
            List<String> selectedSongs = playedSongs.
                Union(unplayedSongs)
                .Except(songSuggest.songBanning.GetBannedIDs())
                .ToList();



            return selectedSongs;
        }

        private List<String> SortSongs(List<String> songs, SongSortCriteria sort)
        {
            switch (sort)
            {
                case SongSortCriteria.Accuracy:
                    songs = Acc(songs);
                    break;
                case SongSortCriteria.Age:
                    songs = Age(songs);
                    break;
                case SongSortCriteria.PP:
                    songs = PP(songs);
                    break;
                case SongSortCriteria.WeightedAge:
                    songs = WeightedAge(songs);
                    break;
                case SongSortCriteria.WorldPercentage:
                    songs = WorldPercentage(songs);
                    break;
                case SongSortCriteria.Random:
                    songs = RandomOrder(songs);
                    break;
                case SongSortCriteria.WorldRank:
                    songs = WorldRank(songs);
                    break;
            }
            return songs;
        }

        //All Sortings are best to worst.

        //Sort the selection by Accuracy
        private List<String> Acc(List<String> selectedSongs)
        {
            //Lets find set scores and order them
            var setScores = selectedSongs
                .Where(songID => songSuggest.activePlayer.scores.ContainsKey(songID))
                .Select(songID => songSuggest.activePlayer.scores[songID])
                .OrderByDescending(score => score.accuracy)
                .Select(score => score.songID)
                .ToList();

            //Any non found scores (unplayed if added) are found here
            var unplayedSongs = selectedSongs
                .Except(setScores)
                .ToList();

            //Unknown scores are treated as 0 pp, and returned along with set scores
            return setScores.Union(unplayedSongs).ToList();
        }

        //Sort the selection by Age
        private List<String> Age(List<String> selectedSongs)
        {
            //Lets find set scores and order them
            var setScores = selectedSongs
                .Where(songID => songSuggest.activePlayer.scores.ContainsKey(songID))
                .Select(songID => songSuggest.activePlayer.scores[songID])
                .OrderByDescending(score => score.timeSet)
                .Select(score => score.songID)
                .ToList();
            
            //Any non found scores (unplayed if added) are found here
            var unplayedSongs = selectedSongs
                .Except(setScores)
                .ToList();

            //Unknown scores are treated as oldest, and returned along with set scores
            return setScores.Union(unplayedSongs).ToList();
        }

        //Sort the selection by PP
        private List<String> PP(List<String> selectedSongs)
        {
            //Lets find set scores and order them
            var setScores = selectedSongs
                .Where(songID => songSuggest.activePlayer.scores.ContainsKey(songID))
                .Select(songID => songSuggest.activePlayer.scores[songID])
                .OrderByDescending(score => score.pp)
                .Select(score => score.songID)
                .ToList();

            //Any non found scores (unplayed if added) are found here
            var unplayedSongs = selectedSongs
                .Except(setScores)
                .ToList();

            //Unknown scores are treated as 0 pp, and returned along with set scores
            return setScores.Union(unplayedSongs).ToList();
        }

        //Sort the selection random, but with preferential toward oldest songs (unplayed are counted as 1 year old)
        private List<String> WeightedAge(List<String> selectedSongs)
        {
            List<WeightedPair> selectedSongPairs = new List<WeightedPair>();

            foreach (var song in selectedSongs)
            {
                DateTime defaultTime = DateTime.Now.AddYears(-1);
                DateTime timeSet = songSuggest.activePlayer.scores.ContainsKey(song) ? songSuggest.activePlayer.scores[song].timeSet : defaultTime;

                double weight = (DateTime.Now - timeSet).TotalSeconds;

                selectedSongPairs.Add(new WeightedPair()
                {
                    Value = song,
                    Weight = weight
                });
            }

            var selection = new WeightedSelection(selectedSongPairs)
                .GetAllRandom()
                .Select(c => (String)c.Value)
                .ToList();
            
            //Weighted gives a priority to older songs, to maintain same default priority we need to put the priority on low day count on the selection.
            selection.Reverse();
            return selection;
        }

        //Sort the selection by worldPercentage
        private List<String> WorldPercentage(List<String> selectedSongs)
        {
            //Lets find set scores and order them
            var setScores = selectedSongs
                .Where(songID => songSuggest.activePlayer.scores.ContainsKey(songID))
                .Select(songID => songSuggest.activePlayer.scores[songID])
                .OrderBy(score => score.rankPercentile)
                .Select(score => score.songID)
                .ToList();

            //Any non found scores (unplayed if added) are found here
            var unplayedSongs = selectedSongs
                .Except(setScores)
                .ToList();

            //Unknown scores are treated as oldest, and returned along with set scores
            return setScores.Union(unplayedSongs).ToList();
        }
        
        //Sort the selection Random
        private List<String> RandomOrder(List<String> selectedSongs)
        {
            Random random = new Random();

            // Start from the end and swap elements randomly
            for (int i = selectedSongs.Count - 1; i > 0; i--)
            {
                int randomIndex = random.Next(0, i + 1);
                var temp = selectedSongs[i];
                selectedSongs[i] = selectedSongs[randomIndex];
                selectedSongs[randomIndex] = temp;
            }

            return selectedSongs;
        }

        //Sort the selection by World Rank
        private List<String> WorldRank(List<String> selectedSongs)
        {
            //Lets find set scores and order them
            var setScores = selectedSongs
                .Where(songID => songSuggest.activePlayer.scores.ContainsKey(songID))
                .Select(songID => songSuggest.activePlayer.scores[songID])
                .OrderBy(score => score.rankScoreSaber)
                .Select(score => score.songID)
                .ToList();

            //Any non found scores (unplayed if added) are found here
            var unplayedSongs = selectedSongs
                .Except(setScores)
                .ToList();

            //Unknown scores are treated as oldest, and returned along with set scores
            return setScores.Union(unplayedSongs).ToList();
        }
    }

    public class WeightedSelection
    {
        private Random rnd = new Random();

        private WeightedSelection leftWS;
        private WeightedSelection rightWS;
        private WeightedPair myPair;
        public Double Weight { get; private set; }
        public int Count { get; private set; }
        private int leftItems;
        private int rightItems;

        public WeightedSelection(List<WeightedPair> weightedPairs)
        {
            //Get first item and store in this item.
            myPair = weightedPairs[0];
            Count = 1;
            Weight = myPair.Weight;

            //Splits incoming in 3 parts, first has been stored here so it is skipped
            //Indexes for remaining are found split in two, extra goes left
            //Increase total by found total for the sides.
            
            int leftStart = 1;
            leftItems = (weightedPairs.Count)/2;
            int rightStart = leftItems+1;
            rightItems = (weightedPairs.Count-1-leftItems);

            if (leftItems > 0)
            {
                leftWS = new WeightedSelection(weightedPairs.GetRange(leftStart, leftItems));
                Count += leftWS.Count;
                Weight += leftWS.Weight;
            }
            if (rightItems > 0)
            {
                rightWS = new WeightedSelection(weightedPairs.GetRange(rightStart, rightItems));
                Count += rightWS.Count;
                Weight += rightWS.Weight;
            }
        }

        public WeightedPair GetRandom()
        {
            return GetAt(rnd.NextDouble()*Weight);
        }

        public WeightedPair GetAt (double selection)
        {
            WeightedPair returnPair = null;

            //No items left
            if (Count == 0) return returnPair;

            Count--;

            //Check if own Item is selected
            if (selection < myPair.Weight)
            {
                returnPair = myPair;

                //Last Item
                if (Count == 0)
                {
                    Weight = 0;
                    myPair = null;
                    return returnPair;
                }

                //Grab first item from largest side.
                if (leftItems >= rightItems)
                {
                    myPair = leftWS.GetAt(0);
                    leftItems--;
                }
                else
                {
                    myPair = rightWS.GetAt(0);
                    rightItems--;
                }

                Weight = Weight - returnPair.Weight;
                return returnPair;
            }

            //Check if selection is in left side
            selection = selection - myPair.Weight;

            if (selection < leftWS.Weight)
            {
                returnPair = leftWS.GetAt(selection);
                leftItems--;
                Weight = Weight - returnPair.Weight;

                //If left is empty, move right to left and clear left
                if (leftItems == 0)
                {
                    leftWS = rightWS;
                    leftItems = rightItems;
                    rightItems = 0;
                    rightWS = null;
                }

                return returnPair;
            }

            //Selection is in right side
            selection = selection - leftWS.Weight;

            returnPair = rightWS.GetAt(selection);
            rightItems--;
            Weight = Weight - returnPair.Weight;

            return returnPair;
        }

        public List<WeightedPair> Take(int count)
        {
            if (count > Count) count = Count;
            List<WeightedPair> returnList = new List<WeightedPair>();

            while (count > 0)
            {
                returnList.Add(GetRandom());
                count--;
            }
            return returnList;
        }

        public List<WeightedPair> GetAllRandom()
        {
            return Take(Count);
        }
    }

    public class WeightedPair
    {
        public Object Value { get; set; }
        public Double Weight { get; set; }
    }
}