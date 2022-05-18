using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using SmartSongSuggest.Configuration;
using SmartSongSuggest.Managers;
using System.ComponentModel;
using System.Reflection;

namespace SmartSongSuggest.UI
{
    [HotReload(RelativePathToLayout = @"Views\SongSuggestLeft.bsml")]
    [ViewDefinition("SmartSongSuggest.UI.Views.SongSuggestLeft.bsml")]
    class SettingsLeftController : BSMLAutomaticViewController, INotifyPropertyChanged
    {
        public static PluginConfig cfgInstance = SettingsController.cfgInstance;


        [UIValue("mod-version")]
        public string modVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        [UIAction("open-kofi")]
        public void OpenKoFi()
        {
            System.Diagnostics.Process.Start("https://ko-fi.com/smartsongsuggest");
        }

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
