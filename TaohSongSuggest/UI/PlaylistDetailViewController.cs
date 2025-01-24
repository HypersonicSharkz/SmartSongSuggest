using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberPlaylistsLib;
using BeatSaberPlaylistsLib.Types;
using HMUI;
using Settings;
using SmartSongSuggest.Configuration;
using SmartSongSuggest.Managers;
using SongSuggestNS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SmartSongSuggest.UI
{
    public class PlaylistDetailViewController : INotifyPropertyChanged
    {
        internal static readonly PlaylistDetailViewController persController = new PlaylistDetailViewController();
        internal LevelPackDetailViewController lpdvc;
        internal static AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollectionsViewController;
        internal static IPlaylist selectedPlaylist;

        public event PropertyChangedEventHandler PropertyChanged;

        [UIParams]
        private readonly BSMLParserParams parserParams;

        [UIComponent("root")]
        private readonly Transform rootTransform;

        [UIComponent("syncurl-button")]
        private readonly NoTransitionsButton syncUrlButton;

        private void ProcessSyncPlaylist()
        {
            Task.Run(() =>
            {
                //Save link to updated playlist in case user selects another during update. (under async thread, we can have a delay when requesting the playlist from web in SongSuggestCore.)
                var pl = selectedPlaylist;

                if (selectedPlaylist == null)
                    return;

                string path = PlaylistManager.DefaultManager.GetManagerForPlaylist(selectedPlaylist).PlaylistPath;
                path = path.Replace(PlaylistManager.DefaultManager.PlaylistPath, "");

                string fileName = selectedPlaylist.Filename;
                string extension = selectedPlaylist.SuggestedExtension;
                SongSuggestManager.toolBox.log?.WriteLine($"Variables: {path} {fileName} {extension}");

                PlaylistPath playlistPath = new PlaylistPath() { FileExtension = extension, FileName = fileName, Subfolders = path };

                SongSuggest.MainInstance.FilterSyncURL(playlistPath, playlistPath);

                //Tell main thread to update the playlist.
                IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() =>
                {
                    SongSuggestManager.UpdatePlaylists(pl);
                });
            });
        }

        public static bool AttachTo(Transform t, LevelPackDetailViewController pack, AnnotatedBeatmapLevelCollectionsViewController collectionsViewController)
        {
            if (t == null)
                return false;

            try
            {
                annotatedBeatmapLevelCollectionsViewController = collectionsViewController;

                BSMLParser.Instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "SmartSongSuggest.UI.Views.PlaylistDetailView.bsml"), t.gameObject, persController);
                persController.rootTransform.localScale *= 0.6f;
                persController.lpdvc = pack;
                persController.lpdvc.didActivateEvent += Lpdvc_didActivateEvent;

                annotatedBeatmapLevelCollectionsViewController.didSelectAnnotatedBeatmapLevelCollectionEvent += collectionSelected;
                collectionSelected(annotatedBeatmapLevelCollectionsViewController.selectedAnnotatedBeatmapLevelPack);
                return true;
            }
            catch (Exception ex)
            {
                Plugin.Log.Error(ex);
                return false;
            }
        }

        private static void collectionSelected(BeatmapLevelPack obj)
        {
            if (obj is PlaylistLevelPack pl)
            {
                selectedPlaylist = pl.playlist;
            } 
            else
            {
                selectedPlaylist = null;
            }

            persController.UpdateView();
        }

        private static void Lpdvc_didActivateEvent(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            persController.UpdateView();
        }

        private void UpdateView()
        {
            var cfg = SettingsController.cfgInstance;
            bool active = selectedPlaylist != null && selectedPlaylist.TryGetCustomData("syncURL", out var outSyncURL) && cfg.ShowSyncURL;
            syncUrlButton.gameObject.SetActive(active);
            //syncUrlButton.gameObject.SetActive(selectedPlaylist != null && selectedPlaylist.TryGetCustomData("syncURL", out var outSyncURL));
        }
    }
}
