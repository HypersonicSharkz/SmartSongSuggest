using BeatSaberMarkupLanguage.MenuButtons;
using HMUI;
using BeatSaberMarkupLanguage;
using SmartSongSuggest.UI;

namespace SmartSongSuggest.Managers
{
    static class UIManager
    {
        public static void Init()
        {
            MenuButtons.instance.RegisterButton(new MenuButton("Smart Song Suggest", "Smart ranked song suggestions", ShowFlow, true));
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
