using Actions;
using ActivePlayerData;
using BanLike;
using FileHandling;
using LinkedData;
using ScoreSabersJson;
using Settings;
using SongLibraryNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaohSongSuggest.Configuration;
using TaohSongSuggest.UI;
using WebDownloading;
using BeatSaberPlaylistsLib;
using System.IO;
using PlaylistNS;
using DataHandling;

namespace TaohSongSuggest.Managers
{
    static class SongSuggestManager
    {

        internal static SongSuggestSettings linkedSettings;
        internal static SongSuggest songSuggest;

        internal static ToolBox toolBox;


        //Method for sending progress info to the UI on the main thread
        static async void UpdateProgess()
        {
            songSuggest.songSuggestCompletion = 0;

            while (songSuggest.songSuggestCompletion != 1)
            {
                TSSFlowCoordinator.settingsView.RefreshProgressBar((float)songSuggest.songSuggestCompletion);
                Console.WriteLine(songSuggest.songSuggestCompletion);
                await Task.Delay(200);
            }
            TSSFlowCoordinator.settingsView.RefreshProgressBar(1);

        }

        public static void Init()
        {
            toolBox = new ToolBox();

            //Check directories
            Directory.CreateDirectory(Path.GetDirectoryName(FileHandler.activePlayerDataPath));        
        }

        public static void SuggestSongs()
        {
            

            Task.Run(() =>
            {
                try
                {
                    IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew( () => TSSFlowCoordinator.Instance.ToggleBackButton(false));
                    
                    PluginConfig cfg = SettingsController.cfgInstance;

                    linkedSettings = new SongSuggestSettings
                    {
                        scoreSaberID = BS_Utils.Gameplay.GetUserInfo.GetUserID(),
                        rankFrom = cfg.fromRank,
                        rankTo = cfg.toRank,
                        ignorePlayedAll = cfg.ignorePlayedAll,
                        ignorePlayedDays = cfg.ignorePlayedDays,
                        styleFocus = cfg.styleFocus,
                        useLikedSongs = cfg.useLikedSongs,
                        fillLikedSongs = cfg.fillLikedSongs
                    };

                    songSuggest = new SongSuggest(toolBox);

                    IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() => UpdateProgess());

                    songSuggest.SuggestedSongs(linkedSettings);

                    songSuggest.songSuggestCompletion = 1;

                    Task.Delay(100);

                    BeatSaberPlaylistsLib.Types.IPlaylist pl;
                    if (PlaylistManager.DefaultManager.TryGetPlaylist("SongSuggest", out pl))
                    {
                        Console.WriteLine("SS found playlist");
                        PlaylistManager.DefaultManager.MarkPlaylistChanged(pl);
                        PlaylistManager.DefaultManager.RefreshPlaylists(true);
                    }

                    IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() => TSSFlowCoordinator.Instance.ToggleBackButton(true));
                }
                catch (Exception e)
                {
                    Plugin.Log.Error(e.Message + " " + e.InnerException);

                    TSSFlowCoordinator.Instance.ToggleBackButton(true);
                }
            });


            
        }

        public static void Oldest100ActivePlayer()
        {
            Task.Run(() =>
            {
                try
                {
                    IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() => TSSFlowCoordinator.Instance.ToggleBackButton(false));

                    OldestSongs oldestSongs = new OldestSongs(toolBox);// { toolBox = this.toolBox};

                    OldestSongSettings settings = new OldestSongSettings
                    {
                        scoreSaberID = BS_Utils.Gameplay.GetUserInfo.GetUserID(),
                        ignoreAccuracyEqualAbove = (double)SettingsController.cfgInstance.old_highest_acc,
                        ignorePlayedDays = SettingsController.cfgInstance.old_oldest_days
                    };

                    oldestSongs.Oldest100ActivePlayer(settings);

                    Task.Delay(200);

                    BeatSaberPlaylistsLib.Types.IPlaylist pl;
                    if (PlaylistManager.DefaultManager.TryGetPlaylist("100 Oldest", out pl))
                    {
                        Console.WriteLine("SS found playlist");
                        PlaylistManager.DefaultManager.MarkPlaylistChanged(pl);
                        PlaylistManager.DefaultManager.RefreshPlaylists(true);
                    }

                    IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() => TSSFlowCoordinator.Instance.ToggleBackButton(true));
                }
                catch (Exception e)
                {
                    Plugin.Log.Error(e.Message + " " + e.InnerException);

                    IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() => TSSFlowCoordinator.Instance.ToggleBackButton(true));
                }
            });


        }
    }
}
