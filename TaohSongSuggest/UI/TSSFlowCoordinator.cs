﻿using BeatSaberMarkupLanguage;
using HMUI;
using SmartSongSuggest.Managers;
using UnityEngine;

namespace SmartSongSuggest.UI
{
    class TSSFlowCoordinator : FlowCoordinator
    {
        internal static SettingsController settingsView;
        internal static SettingsLeftController settingsLeftView;
        internal static MapListController mapListView;

        public static TSSFlowCoordinator Instance;

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            Instance = this;

            if (firstActivation)
            {
                SetTitle("Smart Song Suggest");
                settingsView = BeatSaberUI.CreateViewController<SettingsController>();
                settingsLeftView = BeatSaberUI.CreateViewController<SettingsLeftController>();

                ProvideInitialViewControllers(settingsView, settingsLeftView);

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
