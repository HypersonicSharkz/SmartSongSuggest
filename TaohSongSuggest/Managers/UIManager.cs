using BeatSaberMarkupLanguage.MenuButtons;
using HMUI;
using BeatSaberMarkupLanguage;
using SmartSongSuggest.UI;
using BeatSaberMarkupLanguage.GameplaySetup;

namespace SmartSongSuggest.Managers
{
    static class UIManager
    {
        internal static MenuButton SmartSongSuggestButton;

        public static void Init()
        {
            SmartSongSuggestButton = new MenuButton("Smart Song Suggest", "Smart ranked song suggestions", ShowFlow, true);
            MenuButtons.instance.RegisterButton(SmartSongSuggestButton);
            SmartSongSuggestButton.Interactable = false;
            GameplaySetup.instance.AddTab("Smart Song Suggest", "SmartSongSuggest.UI.Views.SongSuggestTab.bsml", TabViewController.instance);
        }

        internal static FlowCoordinator _parentFlow { get; private set; }
        internal static TSSFlowCoordinator _flow { get; private set; }

        public static void ShowFlow() => ShowFlow(false);
        public static void ShowFlow(bool immediately)
        {
            if (_flow == null)
                _flow = BeatSaberUI.CreateFlowCoordinator<TSSFlowCoordinator>();

            _parentFlow = BeatSaberUI.MainFlowCoordinator.YoungestChildFlowCoordinatorOrSelf();

            BeatSaberUI.PresentFlowCoordinator(_parentFlow, _flow, immediately: immediately);
        }
    }
}
