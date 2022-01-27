using BeatSaberMarkupLanguage.MenuButtons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HMUI;
using BeatSaberMarkupLanguage;
using System.Reflection;
using UnityEngine;
using System.Collections;
using TaohSongSuggest.UI;

namespace TaohSongSuggest.Managers
{
    static class UIManager
    {
        public static void Init()
        {
            MenuButtons.instance.RegisterButton(new MenuButton("Taoh Song Suggest", "Smart ranked song suggestions", ShowFlow, true));
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
