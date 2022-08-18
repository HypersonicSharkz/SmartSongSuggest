using Settings;
using System;
using System.Threading.Tasks;
using SmartSongSuggest.Configuration;
using SmartSongSuggest.UI;
using BeatSaberPlaylistsLib;
using System.IO;
using IPA.Utilities;
using SongSuggestNS;

namespace SmartSongSuggest.Managers
{
    static class SongSuggestManager
    {
        internal static SongSuggest toolBox;

        //Method for sending progress info to the UI on the main thread
        static async void UpdateProgessNew()
        {
            while (toolBox.status.ToLowerInvariant() != "ready")
            {
                TSSFlowCoordinator.settingsView.RefreshProgressBar(toolBox.songSuggest != null ? (float)toolBox.songSuggest.songSuggestCompletion : 0);
                await Task.Delay(200);
            }

            toolBox.status = "Ready";
            TSSFlowCoordinator.settingsView.RefreshProgressBar(1);
        }

        public static void Init()
        {
            Task.Run(() =>
            {
                try
                {
                    string configDir = Path.Combine(UnityGame.UserDataPath, "SmartSongSuggest") + "/";

                    //Check directories
                    Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(configDir, "Players/")));

                    
                    FilePathSettings fps = new FilePathSettings()
                    {
                        activePlayerDataPath = Path.Combine(configDir, "Players/"),
                        bannedSongsPath = configDir,
                        likedSongsPath = configDir,
                        top10kPlayersPath = configDir,
                        playlistPath = Path.Combine(UnityGame.InstallPath, "Playlists/"),
                        songLibraryPath = configDir,
                        filesDataPath = configDir,
                        lastSuggestionsPath = configDir
                    };

                    toolBox = new SongSuggest(fps, "-1");
                }
                catch (Exception e)
                {
                    Plugin.Log.Error(e);
                }
            });
        }

        public static void SuggestSongs(bool p_ignoreNonImprovable = false)
        {
            Task.Run(async () =>
            {
                try
                {
                    await IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew( () => TSSFlowCoordinator.Instance.ToggleBackButton(false));

                    PluginConfig cfg = SettingsController.cfgInstance;

                    FilterSettings filterSettings = new FilterSettings
                    {
                        modifierPP = cfg.modifierPP,
                        modifierStyle = cfg.modifierStyle,
                        modifierOverweight = cfg.modifierOverweight
                    };

                    PlaylistSettings playListSettings = new PlaylistSettings
                    {
                        fileName = "Song Suggest",
                        title = $"Song Suggest",
                        author = "Song Suggest",
                        image = "base64,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",
                    };

                    SongSuggestSettings linkedSettings = new SongSuggestSettings
                    {
                        scoreSaberID = BS_Utils.Gameplay.GetUserInfo.GetUserID(),
                        rankFrom = cfg.fromRank,
                        rankTo = cfg.toRank,
                        ignorePlayedAll = cfg.ignorePlayedAll,
                        ignorePlayedDays = cfg.ignorePlayedDays,
                        requiredMatches = cfg.requiredMatches,
                        useLikedSongs = cfg.useLikedSongs,
                        fillLikedSongs = cfg.fillLikedSongs,
                        ignoreNonImproveable = p_ignoreNonImprovable,
                        playlistSettings = playListSettings,
                        filterSettings = filterSettings
                    };

                    toolBox.status = "Starting Search";

                    IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() => UpdateProgessNew());

                    toolBox.GenerateSongSuggestions(linkedSettings);

                    toolBox.songSuggest.songSuggestCompletion = 1;

                    //Task.Delay(100);

                    await IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() =>
                    {

                        if (toolBox.lowQualitySuggestions)
                        {
                            if (cfg.useLikedSongs)
                            {
                                TSSFlowCoordinator.settingsView.ShowError("Not enough liked songs", @"You do currently not have enough liked songs for the program to find personalized maps, this will result in a less optimal playlist.");
                            }
                            else
                            {
                                TSSFlowCoordinator.settingsView.ShowError("A low amount of links was found when generating your suggestions.", @"Due to this the suggestions may be more random and less personalized.
This warning will disappear on song generation when you complete more suggested songs at a high enough accuracy. A simple way to greatly improve accuracy is ensuring you full swing as much as possible.
If this warning persists your Cached data may be broken, try using the 'CLEAR CACHE'.");
                            }
                        }

                        UpdatePlaylists("Song Suggest");
                        TSSFlowCoordinator.Instance.ToggleBackButton(true);

                    });
                }
                catch (Exception e)
                {
                    CallException(e);
                }
            });


            
        }

        public static void CallException(Exception e)
        {
            Plugin.Log.Error(e);
            TSSFlowCoordinator.Instance.ToggleBackButton(true);
            TSSFlowCoordinator.settingsView.ShowError("Oops something went wrong...", $"Try again later. If issue persist try clearing cache. Error message: {e.Message}");
        }

        public static void Oldest100ActivePlayer()
        {
            Task.Run(() =>
            {
                try
                {
                    IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() => TSSFlowCoordinator.Instance.ToggleBackButton(false));

                    PlaylistSettings playListSettings = new PlaylistSettings
                    {
                        title = "100 Oldest",
                        author = "Song Suggest",
                        fileName = "100 oldest",
                        image = "base64,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"
                    };

                    OldestSongSettings settings = new OldestSongSettings
                    {
                        scoreSaberID = BS_Utils.Gameplay.GetUserInfo.GetUserID(),
                        ignoreAccuracyEqualAbove = (double)SettingsController.cfgInstance.old_highest_acc,
                        ignorePlayedDays = SettingsController.cfgInstance.old_oldest_days,
                        playlistSettings = playListSettings
                    };

                    toolBox.status = "Starting Search";

                    IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() => UpdateProgessNew());

                    toolBox.GenerateOldestSongs(settings);

                    //Task.Delay(200);

                    IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() =>
                    {
                        UpdatePlaylists("100 Oldest");
                        TSSFlowCoordinator.Instance.ToggleBackButton(true);
                    });
                }
                catch (Exception e)
                {
                    Plugin.Log.Error(e);

                    IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() => TSSFlowCoordinator.Instance.ToggleBackButton(true));
                }
            });


        }

        internal static void UpdatePlaylists(string playlist)
        {
            BeatSaberPlaylistsLib.Types.IPlaylist pl;
            if (PlaylistManager.DefaultManager.TryGetPlaylist(playlist, out pl))
            {
                PlaylistManager.DefaultManager.MarkPlaylistChanged(pl);
                PlaylistManager.DefaultManager.RefreshPlaylists(true);
            }
        }
    }
}
