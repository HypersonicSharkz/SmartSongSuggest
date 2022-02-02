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

        public ToolBox(FilePathSettings filePathSettings)
        {
            fileHandler = new FileHandler {toolBox = this};
            fileHandler.UpdatePaths(filePathSettings);

            webDownloader = new WebDownloader {toolBox = this };
            
            songLibrary = new SongLibrary(fileHandler, webDownloader);
            fileHandler.LoadSongLibrary(songLibrary);

            songLiking = new SongLiking(songLibrary)
            {
                songLibrary = this.songLibrary,
                fileHandler = this.fileHandler,
                likedSongs = fileHandler.LoadLikedSongs()
            };

            songBanning = new SongBanning
            {
                songLibrary = this.songLibrary,
                fileHandler = this.fileHandler,
                bannedSongs = fileHandler.LoadBannedSongs()
            };

            status = "Preparing Players Song History";
            //creates an active player
            //Needs loading of the data if user is no longer "-1"
            activePlayer = new ActivePlayer(activePlayerID);

            status = "Checking for new Online Files";
            //prepares files
            webDownloader.UpdateLinkData();
            Console.WriteLine("Ready");
            status = "Ready";
        }

        public void SetActivePlayer(String activePlayerID)
        {
            ActivePlayerPrepareData activePlayerPrepareData = new ActivePlayerPrepareData{ toolBox = this };
            activePlayerPrepareData.SetActivePlayer(activePlayerID);
        }
    }
}
