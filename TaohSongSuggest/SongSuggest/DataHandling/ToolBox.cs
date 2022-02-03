using SongLibraryNS;
using ActivePlayerData;
using FileHandling;
using WebDownloading;
using BanLike;
using System;
using Actions;
using Settings;

namespace DataHandling
{
    public class ToolBox
    {
        public String status { get; set; } = "Uninitialized";
        public ActivePlayer activePlayer { get; set; }
        public String activePlayerID { get; set; } = "-1"; //Default as an unset player.
        public FileHandler fileHandler { get; set; }
        public SongLibrary songLibrary { get; set; }
        public WebDownloader webDownloader { get; set; }
        public SongLiking songLiking { get; set; }
        public SongBanning songBanning { get; set; }

        public ToolBox(FilePathSettings filePathSettings, String userID)
        {
            Console.WriteLine("tool box 1");
            //Set the active players ID
            activePlayerID = userID;

            fileHandler = new FileHandler {toolBox = this};
            fileHandler.UpdatePaths(filePathSettings);

            Console.WriteLine("tool box 2");

            webDownloader = new WebDownloader {toolBox = this };
            
            songLibrary = new SongLibrary(fileHandler, webDownloader);
            fileHandler.LoadSongLibrary(songLibrary);

            Console.WriteLine("tool box 3");

            songLiking = new SongLiking(songLibrary)
            {
                songLibrary = this.songLibrary,
                fileHandler = this.fileHandler,
                likedSongs = fileHandler.LoadLikedSongs()
            };

            Console.WriteLine("tool box 4");

            songBanning = new SongBanning
            {
                songLibrary = this.songLibrary,
                fileHandler = this.fileHandler,
                bannedSongs = fileHandler.LoadBannedSongs()
            };

            Console.WriteLine("tool box 5");

            status = "Checking for new Online Files";
            //prepares files
            webDownloader.UpdateLinkData();

            Console.WriteLine("tool box 6");

            status = "Preparing Players Song History";
            //Creates an active player, and refreshes this players data.
            activePlayer = new ActivePlayer(this);
            //RefreshActivePlayer();

            Console.WriteLine("tool box 7");

            status = "Ready";
        }

        public void RefreshActivePlayer()
        {
            ActivePlayerRefreshData activePlayerRefreshData = new ActivePlayerRefreshData{ toolBox = this };
            activePlayerRefreshData.RefreshActivePlayer(activePlayerID);
        }
    }
}
