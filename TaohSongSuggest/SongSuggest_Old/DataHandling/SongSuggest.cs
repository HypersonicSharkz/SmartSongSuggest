using SongLibraryNS;
using ActivePlayerData;
using FileHandling;
using WebDownloading;
using BanLike;
using System;
using Actions;
using Settings;
using LinkedData;

namespace SongSuggestNS
{
    public class SongSuggest
    {
        public String status { get; set; } = "Uninitialized";
        public ActivePlayer activePlayer { get; set; } //Rework so only 
        public String activePlayerID { get; set; } = "-1"; //Default as an unset player.
        public FileHandler fileHandler { get; set; }
        public SongLibrary songLibrary { get; set; }
        public WebDownloader webDownloader { get; set; }
        public SongLiking songLiking { get; set; }
        public SongBanning songBanning { get; set; }
        public Top10kPlayers top10kPlayers { get; set; }


        //Last used action is stored here if the UI wants to use them for further information than just creation of the playlists
        public RankedSongsSuggest songSuggest { get; private set; }
        public OldestSongs oldestSongs { get; private set; }

        //List of last ranked song suggestions
        public LastRankedSuggestions lastSuggestions { get; set; }

        public SongSuggest(FilePathSettings filePathSettings, String userID)
        {
            //Set the active players ID
            activePlayerID = userID;

            fileHandler = new FileHandler {songSuggest = this, filePathSettings = filePathSettings};

            webDownloader = new WebDownloader {songSuggest = this };

            songLibrary = new SongLibrary { songSuggest = this };
            //Load Song Library from File
            songLibrary.SetLibrary(fileHandler.LoadSongLibrary());

            songLiking = new SongLiking
            {
                songSuggest = this,
                likedSongs = fileHandler.LoadLikedSongs()
            };

            songBanning = new SongBanning
            {
                songSuggest = this,
                bannedSongs = fileHandler.LoadBannedSongs()
            };

            status = "Checking for new Online Files";
            //prepares files
            webDownloader.UpdateLinkData();

            status = "Preparing Players Song History";
            //Creates an empty active player object
            activePlayer = new ActivePlayer();
            lastSuggestions = new LastRankedSuggestions {songSuggest = this };
            lastSuggestions.Load();

            status = "Preparing Link Data";
            //Load Link Data
            top10kPlayers = new Top10kPlayers { songSuggest = this };
            top10kPlayers.Load();

            status = "Ready";
        }

        public void GenerateSongSuggestions(SongSuggestSettings settings)
        {
            //Refresh Player Data
            RefreshActivePlayer();

            //Create the Song Suggestion (so once the creation has been made additional information can be kept and loaded from it.
            songSuggest = new RankedSongsSuggest
            {
                songSuggest = this,
                settings = settings,
            };
            songSuggest.SuggestedSongs();
            
            //Update nameplate rankings, and save them.
            lastSuggestions.lastSuggestions = songSuggest.sortedSuggestions;
            lastSuggestions.Save();

            status = "Ready";
        }

        public void ClearSongSuggestions()
        {
            songSuggest = null;
        }

        public void GenerateOldestSongs(OldestSongSettings settings)
        {
            oldestSongs = new OldestSongs(this);
            oldestSongs.Oldest100ActivePlayer(settings);
            status = "Ready";
        }

        public void ClearOldestSongs()
        {
            oldestSongs = null;
        }

        public void RefreshActivePlayer()
        {
            ActivePlayerRefreshData activePlayerRefreshData = new ActivePlayerRefreshData{ songSuggest = this };
            activePlayerRefreshData.RefreshActivePlayer();
        }

        public String GetSongRanking(String hash, String difficulty)
        {
            return lastSuggestions.GetRank(hash,difficulty);
        }

        public String GetSongRankingCount()
        {
            return lastSuggestions.GetRankCount();
        }
    }
}
