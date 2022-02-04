using HarmonyLib;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using SongDetailsCache;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SmartSongSuggest.Configuration;
using SmartSongSuggest.Managers;
using SmartSongSuggest.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;

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
            harmony = new Harmony("HypersonicSharkz.BeatSaber.SmartSongSuggest");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            UIManager.Init();
            SongSuggestManager.Init();
            songDetails = SongDetails.Init().Result;
        }

        [OnExit]
        public void OnApplicationQuit()
        {
            harmony.UnpatchSelf();
        }
    }
}
