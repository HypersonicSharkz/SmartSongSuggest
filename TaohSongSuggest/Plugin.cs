using HarmonyLib;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using SongDetailsCache;
using System.Reflection;
using SmartSongSuggest.Configuration;
using SmartSongSuggest.Managers;
using SmartSongSuggest.UI;
using IPALogger = IPA.Logging.Logger;
using UnityEngine;
using System.Collections;

namespace SmartSongSuggest
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        internal static IPALogger Log { get; private set; }
        internal static Harmony harmony { get; private set; }

        internal static SongDetails songDetails { get; private set; }

        [Init]
        /// <summary>
        /// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
        /// [Init] methods that use a Constructor or called before regular methods like InitWithConfig.
        /// Only use [Init] with one Constructor.
        /// </summary>
        public void Init(IPALogger logger, Config conf)
        {
            Log = logger;
            Log.Info("SmartSongSuggest initialized.");
            SettingsController.cfgInstance = conf.Generated<PluginConfig>();
        }

        [OnStart]
        public void OnApplicationStart()
        {
            Log.Info("1");
            harmony = new Harmony("HypersonicSharkz.BeatSaber.SmartSongSuggest");
            Log.Info("2");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.Info("3");

            UIManager.Init();
            Log.Info("4");
            SongSuggestManager.Init();
            Log.Info("5");
            songDetails = SongDetails.Init().Result;
            Log.Info("6");
        }

        [OnExit]
        public void OnApplicationQuit()
        {
            harmony.UnpatchSelf();
        }
    }
}
