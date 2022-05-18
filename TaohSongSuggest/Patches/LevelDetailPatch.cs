using HarmonyLib;
using System.Collections;
using SmartSongSuggest.UI;
using UnityEngine;

namespace SmartSongSuggest.Patches
{
    [HarmonyPatch(typeof(StandardLevelDetailViewController), "DidActivate")]
    static class LevelDetailPatch
    {
        static void Prefix(StandardLevelDetailViewController __instance, bool firstActivation)
        {
            if (!firstActivation)
                return;

            SharedCoroutineStarter.instance.StartCoroutine(InitDelayed(__instance.transform));
        }

        static IEnumerator InitDelayed(Transform t)
        {
            yield return new WaitForEndOfFrame();
            LevelDetailViewController.AttachTo(t.Find("LevelDetail"));
        }
    }
}
