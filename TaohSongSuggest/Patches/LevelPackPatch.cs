using HarmonyLib;
using System.Collections;
using SmartSongSuggest.UI;
using UnityEngine;
using SmartSongSuggest.Managers;
using System;
using SongSuggestNS;
using Newtonsoft.Json.Linq;
using BeatSaberPlaylistsLib;

namespace SmartSongSuggest.Patches
{
    [HarmonyPatch(typeof(LevelPackDetailViewController), "DidActivate")]
    static class LevelPackPatch
    {
        static void Prefix(LevelPackDetailViewController __instance, bool firstActivation)
        {
            //Informs the SongSuggestManager a new assignment is needed.
            if (firstActivation)
            {
                Helper.levelPackDetailViewController = __instance;
                Helper.TryAttach();
            }
        }
    }

    [HarmonyPatch(typeof(AnnotatedBeatmapLevelCollectionsViewController), "DidActivate")]
    static class AnnotatedLevelPackPatch
    {
        static void Prefix(AnnotatedBeatmapLevelCollectionsViewController __instance, bool firstActivation)
        {
            //Informs the SongSuggestManager a new assignment is needed.
            if (firstActivation)
            {
                Helper.annotatedBeatmapLevelCollections = __instance;
                Helper.TryAttach();
            }
        }
    }


    static class Helper
    {
        public static AnnotatedBeatmapLevelCollectionsViewController annotatedBeatmapLevelCollections;
        public static LevelPackDetailViewController levelPackDetailViewController;

        public static void TryAttach()
        {
            if (!annotatedBeatmapLevelCollections || !levelPackDetailViewController)
                return;

            SharedCoroutineStarter.instance.StartCoroutine(InitDelayed());
        }

        static IEnumerator InitDelayed()
        {
            yield return new WaitForEndOfFrame();

            PlaylistDetailViewController.AttachTo(levelPackDetailViewController.transform.Find("Detail"), levelPackDetailViewController, annotatedBeatmapLevelCollections);
        }
    }
}
