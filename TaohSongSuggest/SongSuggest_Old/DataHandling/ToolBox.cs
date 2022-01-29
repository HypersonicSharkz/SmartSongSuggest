using SongLibraryNS;
using ActivePlayerData;
using FileHandling;
using WebDownloading;
using BanLike;
using System;
using Actions;

namespace DataHandling
{
    public class ToolBox
    {
        public ActivePlayer activePlayer { get; set; }
        public FileHandler fileHandler { get; set; }
        public SongLibrary songLibrary { get; set; }
        public WebDownloader webDownloader { get; set; }
        public SongLiking songLiking { get; set; }
        public SongBanning songBanning { get; set; }

        public ToolBox()
        {
            fileHandler = new FileHandler();
            webDownloader = new WebDownloader();
            
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

            activePlayer = new ActivePlayer("-1");

        }

        public void SetActivePlayer(String activePlayerID)
        {
            ActivePlayerPrepareData activePlayerPrepareData = new ActivePlayerPrepareData{ toolBox = this };
            activePlayerPrepareData.SetActivePlayer(activePlayerID);
        }
    }
}
