using HarmonyLib;
using System.Collections;
using SmartSongSuggest.UI;
using UnityEngine;
using SmartSongSuggest.Managers;
using System;

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
                SharedCoroutineStarter.instance.StartCoroutine(InitDelayed(__instance.transform, __instance));
            }
        }

        static IEnumerator InitDelayed(Transform t, LevelPackDetailViewController l)
        {
            yield return new WaitForEndOfFrame();
            PlaylistDetailViewController.AttachTo(t.Find("Detail"), l);
        }
    }
}
