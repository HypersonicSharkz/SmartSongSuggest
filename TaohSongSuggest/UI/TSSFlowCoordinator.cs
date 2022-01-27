using BeatSaberMarkupLanguage;
using HMUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaohSongSuggest.Managers;
using UnityEngine;

namespace TaohSongSuggest.UI
{
    class TSSFlowCoordinator : FlowCoordinator
    {
        internal static SettingsController settingsView;
        internal static MapListController mapListView;

        public static TSSFlowCoordinator Instance;

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            Instance = this;

            if (firstActivation)
            {
                SetTitle("Taoh Song Suggest");
                settingsView = BeatSaberUI.CreateViewController<SettingsController>();

                ProvideInitialViewControllers(settingsView);

                showBackButton = true;
            }
        }

        public void ToggleBackButton(bool enable)
        {
            GameObject backBtn = GameObject.Find("MenuCore/UI/ScreenSystem/TopScreen/TitleViewController/BackButton");
            backBtn.GetComponent<NoTransitionsButton>().interactable = enable;

            settingsView.SetButtonsEnable(enable);
        }

        protected override void BackButtonWasPressed(ViewController topViewController) 
        {
            UIManager._parentFlow.DismissFlowCoordinator(this, () =>
            {
                SongCore.Loader.Instance.RefreshSongs(false);
            }, ViewController.AnimationDirection.Horizontal, false);
        }
    }
}
