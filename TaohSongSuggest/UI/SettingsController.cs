using System.ComponentModel;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using SmartSongSuggest.Configuration;
using SmartSongSuggest.Managers;
using TMPro;
using UnityEngine;
using BeatSaberMarkupLanguage.Parser;

namespace SmartSongSuggest.UI
{
    [HotReload(RelativePathToLayout = @"Views\SongSuggestMain.bsml")]
    [ViewDefinition("SmartSongSuggest.UI.Views.SongSuggestMain.bsml")]
    class SettingsController : BSMLAutomaticViewController, INotifyPropertyChanged
    {
        public static PluginConfig cfgInstance;

        [UIParams]
        private readonly BSMLParserParams parserParams;

        [UIComponent("bgProgress")] 
        public ImageView bgProgress;

        [UIComponent("statusText")]
        public TextMeshProUGUI statusComponent;

        [UIComponent("SuggestBTN")]
        public NoTransitionsButton suggestBTN;

        [UIComponent("OldestBTN")]
        public NoTransitionsButton oldestBTN;

        [UIComponent("SuggestBTNNon")]
        public NoTransitionsButton suggestBTNNon;

        bool _suggestShow = true;

        [UIValue("suggest-show")]
        public bool ShowSuggestSettings
        {
            get => _suggestShow;
            set
            {
                _suggestShow = value;
                NotifyPropertyChanged(nameof(SuggestColor));
                NotifyPropertyChanged(nameof(OldestColor));
                NotifyPropertyChanged(nameof(ShowOldestSettings));
                NotifyPropertyChanged(nameof(ShowSuggestSettings));
            }
        }

        [UIValue("oldest-show")]
        public bool ShowOldestSettings => !_suggestShow;


        [UIValue("color-suggest")]
        public string SuggestColor => _suggestShow ? "#00ff00" : "white";


        string _errorHeader = "";
        [UIValue("error-header")]
        public string ErrorHeader => _errorHeader;

        string _errorDescription = "";
        [UIValue("error-description")]
        public string ErrorDescription => _errorDescription;



        [UIValue("color-oldest")]
        public string OldestColor => _suggestShow ? "white" : "#00ff00";

        [UIAction("settingsOldest")]
        void so()
        {
            ShowSuggestSettings = false;
        }

        [UIAction("settingsSuggest")]
        void ss()
        {
            ShowSuggestSettings = true;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            RefreshProgressBar(0f);
        }

        void GenerateOldest()
        {
            SongSuggestManager.Oldest100ActivePlayer();
        }

        void GeneratePlaylist()
        {
            SongSuggestManager.SuggestSongs(cfgInstance.removeOptimizedScores);
        }

        public void SetButtonsEnable(bool enable)
        {
            suggestBTN.interactable = enable;
            oldestBTN.interactable = enable;
            suggestBTNNon.interactable = enable;
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
            } catch
            {

            }

        }

        public void ShowError(string header, string description)
        {
            _errorHeader = header;
            _errorDescription = description;

            NotifyPropertyChanged(nameof(ErrorDescription));
            NotifyPropertyChanged(nameof(ErrorHeader));

            this.parserParams.EmitEvent("close-modal");
            this.parserParams.EmitEvent("open-modal");
        }
    }
}
