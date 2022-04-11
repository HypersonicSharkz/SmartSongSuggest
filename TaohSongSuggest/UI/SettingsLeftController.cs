using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using SmartSongSuggest.Configuration;
using SmartSongSuggest.Managers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SmartSongSuggest.UI
{
    [HotReload(RelativePathToLayout = @"Views\SongSuggestLeft.bsml")]
    [ViewDefinition("SmartSongSuggest.UI.Views.SongSuggestLeft.bsml")]
    class SettingsLeftController : BSMLAutomaticViewController, INotifyPropertyChanged
    {
        public static PluginConfig cfgInstance = SettingsController.cfgInstance;


        [UIValue("mod-version")]
        public string modVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public void ClearCache()
        {
            SongSuggestManager.toolBox.ClearUser();
        }

        public void ClearBans()
        {
            SongSuggestManager.toolBox.ClearBan();
        }

        public void ClearLikes()
        {
            SongSuggestManager.toolBox.ClearLiked();
        }
    }
}
