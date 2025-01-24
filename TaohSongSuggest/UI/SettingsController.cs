using System.ComponentModel;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using SmartSongSuggest.Configuration;
using SmartSongSuggest.Managers;
using TMPro;
using UnityEngine;
using BeatSaberMarkupLanguage.Parser;
using System.Collections.Generic;
using System.Linq;
using SongSuggestNS;
using System.Collections;
using System;
using Newtonsoft.Json;
using BeatSaberMarkupLanguage;

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
            SongSuggestManager.SuggestSongs();
        }

        public void SetButtonsEnable(bool enable)
        {
            suggestBTN.interactable = enable;
            oldestBTN.interactable = enable;
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

        public void ShowError(string header, string description)
        {
            _errorHeader = header;
            _errorDescription = description;

            NotifyPropertyChanged(nameof(ErrorDescription));
            NotifyPropertyChanged(nameof(ErrorHeader));

            this.parserParams.EmitEvent("close-modal");
            this.parserParams.EmitEvent("open-modal");
        }

        [UIValue("contents")]
        public IList Contents
        {
            get
            {
                List<SongCategoryDisplay> contents = new List<SongCategoryDisplay>();

                IEnumerable<SongCategory> songCategories = Enum.GetValues(typeof(SongCategory)).Cast<SongCategory>();
                bool update = false;

                foreach (SongCategory category in songCategories)
                {
                    SongCategoryDisplay savedCategory = cfgInstance.SongCategories.FirstOrDefault(c => c.SongCategory == category);
                    if (savedCategory == null)
                    {
                        savedCategory = new SongCategoryDisplay();
                        savedCategory.SongCategory = category;
                        cfgInstance.SongCategories.Add(savedCategory);
                        update = true;
                    }

                    contents.Add(savedCategory);
                }

                if (update)
                    cfgInstance.Changed();

                return contents;
            }
        }

        [UIAction("settings-click")]
        private void ShowSettings()
        {
            this.parserParams.EmitEvent("close-settings");
            this.parserParams.EmitEvent("open-settings");
        }

        [UIAction("accsaber-click")]
        private void ShowScoreSaber()
        {
            this.parserParams.EmitEvent("close-accsaber");
            this.parserParams.EmitEvent("open-accsaber");
        }


        [UIAction("categories-click")]
        private void ShowCategories()
        {
            this.parserParams.EmitEvent("close-categories");
            this.parserParams.EmitEvent("open-categories");
        }

        [UIValue("category-size")]
        private int CategorySize
        {
            get
            {
                int count = Enum.GetValues(typeof(SongCategory)).Length;
                return count * 8 + 15;
            }
        }
    }
}
