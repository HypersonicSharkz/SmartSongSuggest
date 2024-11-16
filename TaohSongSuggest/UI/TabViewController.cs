using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Util;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatSaberPlaylistsLib.Types;
using HMUI;
using Settings;
using SmartSongSuggest.Configuration;
using SmartSongSuggest.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace SmartSongSuggest.UI
{
    internal class TabViewController : NotifiableSingleton<TabViewController>
    {
        [UIComponent("bgProgress")]
        public ImageView bgProgress;

        [UIComponent("statusText")]
        public TextMeshProUGUI statusComponent;

        [UIValue("loaded")]
        public bool loaded
        {
            get => SongSuggestManager.toolBox != null;
        }

        public async void Initialize()
        {
            while (SongSuggestManager.toolBox == null)
            {
                RefreshProgressBar(0);
                await Task.Delay(200);
            }

            RefreshProgressBar(0);

            NotifyPropertyChanged(nameof(loaded));
        }

        [UIAction("RegenSuggest")]
        public void RegenSuggest()
        {
            Task.Run(() =>
            {
                try
                {
                    //await IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() => TSSFlowCoordinator.Instance.ToggleBackButton(false));
                    while (SongSuggestManager.toolBox == null)
                        Thread.Sleep(500);

                    SongSuggestManager.toolBox.status = "Starting Search";

                    IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() => UpdateProgessNew());

                    PluginConfig cfg = SettingsController.cfgInstance;

                    SongSuggestSettings linkedSettings = SongSuggestManager.GetSongSuggestSettingsOld(cfg);

                    SongSuggestManager.toolBox.GenerateSongSuggestions(linkedSettings);

                    SongSuggestManager.toolBox.songSuggest.songSuggestCompletion = 1;

                    //Task.Delay(100);

                    IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() =>
                    {
                        IPlaylist pl = SongSuggestManager.UpdatePlaylists("Song Suggest");
                        if (pl == null)
                            return;

                        var lfnc = GameObject.FindObjectOfType<LevelFilteringNavigationController>();
                        lfnc.SelectAnnotatedBeatmapLevelCollection(pl.PlaylistLevelPack);
                    });
                }
                catch (Exception e)
                {
                    Plugin.Log.Error(e);
                }
            });
        }

        [UIAction("RegenOldest")]
        public void RegenOldNew()
        {
            Task.Run(() =>
            {
                try
                {
                    while (SongSuggestManager.toolBox == null)
                        Thread.Sleep(500);

                    SongSuggestManager.toolBox.status = "Starting Search";

                    IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() => UpdateProgessNew());

                    var ui = SettingsController.cfgInstance;

                    OldAndNewSettings settings = SongSuggestManager.GetOldAndNewSettings(ui);

                    SongSuggestManager.toolBox.GenerateOldestSongs(settings);

                    //Task.Delay(200);

                    IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() =>
                    {
                        IPlaylist pl = SongSuggestManager.UpdatePlaylists("Old and New");
                        if (pl == null)
                            return;

                        var lfnc = GameObject.FindObjectOfType<LevelFilteringNavigationController>();
                        lfnc.SelectAnnotatedBeatmapLevelCollection(pl.PlaylistLevelPack);
                    });
                }
                catch (Exception e)
                {
                    Plugin.Log.Error(e);
                }
            });
        }

        [UIAction("Settings")]
        public void Settings()
        {
            UIManager.ShowFlow();
        }

        public async void UpdateProgessNew()
        {
            while (SongSuggestManager.toolBox.status.ToLowerInvariant() != "ready")
            {
                RefreshProgressBar(SongSuggestManager.toolBox.songSuggest != null ? (float)SongSuggestManager.toolBox.songSuggest.songSuggestCompletion : 0);
                await Task.Delay(200);
            }

            SongSuggestManager.toolBox.status = "Ready";
            RefreshProgressBar(0);
        }

        int dots = 0;
        public void RefreshProgressBar(float prog)
        {
            try
            {
                dots = (dots % 3) + 1;
                statusComponent.text = SongSuggestManager.toolBox != null ? SongSuggestManager.toolBox.status : "Loading" + new string('.', dots);

                bgProgress.color = Color.green;

                var x = (bgProgress.gameObject.transform as RectTransform);
                if (x == null)
                    return;

                x.anchorMax = new Vector2(prog, 1);

                x.ForceUpdateRectTransforms();
            }
            catch
            {

            }

        }
    }
}
