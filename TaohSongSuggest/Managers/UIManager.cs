using BeatSaberMarkupLanguage.MenuButtons;
using HMUI;
using BeatSaberMarkupLanguage;
using SmartSongSuggest.UI;
using BeatSaberMarkupLanguage.GameplaySetup;
using System.Threading.Tasks;
using System.Threading;

namespace SmartSongSuggest.Managers
{
    static class UIManager
    {
        public static void Init()
        {
            GameplaySetup.instance.AddTab("Smart Song Suggest", "SmartSongSuggest.UI.Views.SongSuggestTab.bsml", TabViewController.instance);
            TabViewController.instance.Initialize();
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
