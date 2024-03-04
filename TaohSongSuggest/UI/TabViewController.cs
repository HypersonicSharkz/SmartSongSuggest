using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
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

        [UIAction("RegenSuggest")]
        public void RegenSuggest()
        {
            Task.Run(() =>
            {
                try
                {
                    //await IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() => TSSFlowCoordinator.Instance.ToggleBackButton(false));

                    PluginConfig cfg = SettingsController.cfgInstance;

                    SongSuggestSettings linkedSettings = SongSuggestManager.GetSongSuggestSettingsOld(cfg);

                    SongSuggestManager.toolBox.status = "Starting Search";

                    IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() => UpdateProgessNew());

                    SongSuggestManager.toolBox.GenerateSongSuggestions(linkedSettings);

                    SongSuggestManager.toolBox.songSuggest.songSuggestCompletion = 1;

                    //Task.Delay(100);

                    IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() =>
                    {
                        IPlaylist pl = SongSuggestManager.UpdatePlaylists("Song Suggest");
                        if (pl == null)
                            return;

                        var lfnc = GameObject.FindObjectOfType<LevelFilteringNavigationController>();
                        lfnc.SelectAnnotatedBeatmapLevelCollection(pl);
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
                    var ui = SettingsController.cfgInstance;

                    OldAndNewSettings settings = SongSuggestManager.GetOldAndNewSettings(ui);

                    SongSuggestManager.toolBox.status = "Starting Search";

                    IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() => UpdateProgessNew());

                    SongSuggestManager.toolBox.GenerateOldestSongs(settings);

                    //Task.Delay(200);

                    IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() =>
                    {
                        IPlaylist pl = SongSuggestManager.UpdatePlaylists("Old and New");
                        if (pl == null)
                            return;

                        var lfnc = GameObject.FindObjectOfType<LevelFilteringNavigationController>();
                        lfnc.SelectAnnotatedBeatmapLevelCollection(pl);
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

        public void RefreshProgressBar(float prog)
        {
            try
            {
                statusComponent.text = SongSuggestManager.toolBox.status;

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
