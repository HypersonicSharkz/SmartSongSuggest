using SongLibraryNS;
using ActivePlayerData;
using FileHandling;
using WebDownloading;
using BanLike;
using System;
using Actions;
using Settings;
using LinkedData;
using System.IO;
using System.Collections.Generic;
using Data;
using Newtonsoft.Json;

namespace SongSuggestNS
{
    public class SongSuggest
    {
        private SongCategoryTranslate songCategoryTranslation = new SongCategoryTranslate();
        public String status { get; set; } = "Uninitialized";
        public ActivePlayer activePlayer { get; set; }
        //Default as an unset player. Dump ID here and next RefreshActivePlayer() updates it.
        public String activePlayerID { get; set; } = "-1";
        public FileHandler fileHandler { get; set; }
        public SongLibrary songLibrary { get; set; }
        public WebDownloader webDownloader { get; set; }
        public SongLiking songLiking { get; set; }
        public SongBanning songBanning { get; set; }
        public Top10kPlayers top10kPlayers { get; set; }

        //Last used Song Evaluation is stored here if the UI wants to use them for further information than just creation of the playlists
        public RankedSongsSuggest songSuggest { get; private set; } = null;
        public OldAndNew oldestSongs { get; private set; }

        //List of last ranked song suggestions
        public LastRankedSuggestions lastSuggestions { get; set; }

        //Boolean set to true if the quality of the found songs was not high enough
        //e.g. Had to remove the betterAcc and/or songs was missing from generating 50 suggestions.
        public Boolean lowQualitySuggestions { get; set; } = false;

        //Log Details Target (null means it is off), else set the writer here.
        public TextWriter log = null;

        //Initializes the class, different Constructors calls this.
        private void Initialize(FilePathSettings filePathSettings, String userID, TextWriter log)
        {
            //Enable Log
            this.log = log;
            log?.WriteLine("Log Enabled in Constructor");

            //Set the active players ID
            activePlayerID = userID;

            fileHandler = new FileHandler { songSuggest = this, filePathSettings = filePathSettings };

            webDownloader = new WebDownloader { songSuggest = this };
            
            status = "Checking Cached Data against new Online Files";
            //Validate file versions and checks for new data.
            ValidateCacheFiles();

            //Load data from disk.

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

            status = "Preparing Players Song History";
            //Creates an empty active player object
            activePlayer = new ActivePlayer();
            lastSuggestions = new LastRankedSuggestions { songSuggest = this };
            lastSuggestions.Load();

            status = "Preparing Link Data";
            //Load Link Data
            top10kPlayers = new Top10kPlayers { songSuggest = this };
            top10kPlayers.Load();

            status = "Ready";

        }

        //Validate CacheFiles and download new versions if available.
        private void ValidateCacheFiles()
        {
            try
            {
                //Stores if any format has been updated, and if saves the updated formats at the end
                bool updated = false;

                //Load current file structure version and generate a version for the expected versions.
                FileFormatVersions fileFormatDiskVersion = fileHandler.LoadFileFormatVersions();

                FileFormatVersions fileFormatExpectedVersion = new FileFormatVersions()
                {
                    top10kVersion = Top10kPlayers.FormatVersion,
                    activePlayerVersion = ActivePlayer.FormatVersion,
                    songLibraryVersion = SongLibrary.FormatVersion
                };

                //Load web and file of last downloaded version.
                FilesMeta diskVersion = fileHandler.LoadFilesMeta();
                FilesMeta cacheFilesWebVersion = webDownloader.GetFilesMeta();

                //Perform checks if anything needs updating
                //PlayerCache needs update, if the format has changed, or there is a new major update of the 10k data.
                bool formatChange = fileFormatDiskVersion.activePlayerVersion != fileFormatExpectedVersion.activePlayerVersion;
                bool contentChange = diskVersion.Major(FilesMetaType.Top10kVersion) != cacheFilesWebVersion.Major(FilesMetaType.Top10kVersion);
                if (formatChange || contentChange)
                {
                    if (!fileHandler.CheckPlayerRefresh()) fileHandler.TogglePlayerRefresh();
                    updated = true;
                    log?.WriteLine("Marked Playerdata for Refresh");
                }

                //SongLibrary is checked
                //All Song library data changes are major. (New Songs/Reweights).
                //All songs are expected to be up to date after an update, so there are no incremental, new songs only.
                formatChange = fileFormatDiskVersion.songLibraryVersion != fileFormatExpectedVersion.songLibraryVersion;
                contentChange = diskVersion.Major(FilesMetaType.SongLibraryVersion) != cacheFilesWebVersion.Major(FilesMetaType.SongLibraryVersion);
                if (formatChange || contentChange)
                {
                    List<Song> songs = webDownloader.GetSongLibrary();//new List<Song>();
                    fileHandler.SaveSongLibrary(songs);
                    //songLibrary.SetLibrary(songs);
                    updated = true;
                    log?.WriteLine("Downloaded and Updated Song Library");
                }

                //top10kdata is checked
                //New top10k data needs to be downloaded in case of any change to content.
                formatChange = fileFormatDiskVersion.top10kVersion != fileFormatExpectedVersion.top10kVersion;
                contentChange = diskVersion.top10kVersion != cacheFilesWebVersion.top10kVersion;
                if (formatChange || contentChange)
                {
                    List<Top10kPlayer> top10kPlayerData = webDownloader.GetTop10kPlayers();
                    fileHandler.SaveLinkedData(top10kPlayerData);
                    //top10kPlayers.Load();

                    updated = true;
                    log?.WriteLine("Downloaded and Updated top10k data");
                }

                //Save the new local data version if any updates has been completed. If anything fails next restart should attempt full update again.
                if (updated)
                {
                    fileHandler.SaveFilesMeta(cacheFilesWebVersion);
                    fileHandler.SaveFilesFormatVersions(fileFormatExpectedVersion);
                }
            }
            catch
            {
            }
        }

        //Constructor Where log can be enabled.
        public SongSuggest(FilePathSettings filePathSettings, String userID, TextWriter log)
        {
            Initialize(filePathSettings, userID, log);
        }

        //Old Constructor where Log Enabling/Disabling was not possible.
        public SongSuggest(FilePathSettings filePathSettings, String userID)
        {
            Initialize(filePathSettings, userID, null);
        }

        public void GenerateSongSuggestions(SongSuggestSettings settings)
        {
            //Refresh Player Data (Skip if test user ID -100)
            activePlayerID = settings.scoreSaberID;
            if (activePlayerID != "-100") RefreshActivePlayer();

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

        //Requires a RankedSongsSuggest has been performed, then it evaluates the linked songs without updating the user via new settings.
        //**Consider checks for updates of user, and that RankedSongsSuggest has already been performed**
        public void Recalculate(SongSuggestSettings settings)
        {
            if (songSuggest == null) return;
            songSuggest.settings = settings;
            songSuggest.Recalculate();
        }

        public void ClearSongSuggestions()
        {
            songSuggest = null;
        }

        public void GenerateOldestSongs(OldAndNewSettings settings)
        {
            //Refresh Player Data
            activePlayerID = settings.scoreSaberID;
            RefreshActivePlayer();

            oldestSongs = new OldAndNew(this);
            oldestSongs.GeneratePlaylist(settings);
            status = "Ready";
        }

        public void ClearOldestSongs()
        {
            oldestSongs = null;
        }

        //Makes sure the active players data is updated (Load Cache, Reset Cache if needed, Download new data from web).
        public void RefreshActivePlayer()
        {
            ActivePlayerRefreshData activePlayerRefreshData = new ActivePlayerRefreshData { songSuggest = this };
            activePlayerRefreshData.RefreshActivePlayer();
        }

        //Get the placement of a specific song in last RankedSongSuggest, "" if not given a rank.
        public String GetSongRanking(String hash, String difficulty)
        {
            return lastSuggestions.GetRank(hash, difficulty);
        }

        //Amount of linked songs in last RankedSongSuggest evaluation
        public String GetSongRankingCount()
        {
            return lastSuggestions.GetRankCount();
        }

        //Clear all Banned Songs
        public void ClearBan()
        {
            songBanning = new SongBanning()
            {
                songSuggest = this
            };
            fileHandler.SaveBannedSongs(songBanning.bannedSongs);
        }

        //Clears liked Songs
        public void ClearLiked()
        {
            songLiking = new SongLiking()
            {
                songSuggest = this
            };
            fileHandler.SaveLikedSongs(songLiking.likedSongs);
        }

        //Sets a reminder for next RefreshActivePlayer() to perform a full reload of the users data.
        public void ClearUser()
        {
            //Set Reminder file if missing (Can be set to true multiple times, hence check if already set)
            if (!fileHandler.CheckPlayerRefresh()) fileHandler.TogglePlayerRefresh();
        }

        public String SongCategoryText(String lookup)
        {
            if (!songCategoryTranslation.translation.ContainsKey(lookup)) return lookup;
            return songCategoryTranslation.translation[lookup];
        }
    }

    [Flags]
    public enum SongCategory
    {
        ScoreSaber = 1,          
        AccSaberTrue = 2,
        AccSaberStandard = 4,
        AccSaberTech = 8,
        BrokenDownloads = 16          
    }
    public enum SongSortCriteria
    {
        None = 0,
        Age = 1,
        Accuracy = 2,
        PP = 3,
        Star = 4,
        Complexity = 5,
        WorldRank = 6,
        WorldPercentage = 7
    }

    public class SongCategoryTranslate
    {
        public SortedDictionary<string,string> translation = new SortedDictionary<string,string>();

        public SongCategoryTranslate()
        {
            translation.Add($"{(SongCategory)1}Label", "Score Saber");
            translation.Add($"{(SongCategory)1}Hover", "Ranked Score Saber Songs");
            translation.Add($"{(SongCategory)2}Label", "AccSaber - True");
            translation.Add($"{(SongCategory)2}Hover", "AccSabers True Acc Leaderboard");
            translation.Add($"{(SongCategory)4}Label", "AccSaber - Standard");
            translation.Add($"{(SongCategory)4}Hover", "AccSabers Standard Acc Leaderboard");
            translation.Add($"{(SongCategory)8}Label", "AccSaber - Tech");
            translation.Add($"{(SongCategory)8}Hover", "AccSabers Tech Acc Leaderboard");
            translation.Add($"{(SongCategory)16}Label", "Broken Downloads");
            translation.Add($"{(SongCategory)16}Hover", "Songs that may break in download for various reasons. Turn on if you do not mind a Missing Download icon and/or have the songs already.");
        }
    }
}
