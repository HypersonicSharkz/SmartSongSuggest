using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartSongSuggest.UI;
using UnityEngine;

namespace SmartSongSuggest.Patches
{
    [HarmonyPatch(typeof(StandardLevelDetailViewController), "DidActivate")]
    static class LevelDetailPatch
    {
        static void Prefix(StandardLevelDetailViewController __instance, bool firstActivation)
        {
            Plugin.Log.Info("LCNC activated");

            if (!firstActivation)
                return;

            Plugin.Log.Info("LCNC first");

            SharedCoroutineStarter.instance.StartCoroutine(InitDelayed(__instance.transform));
        }

        static IEnumerator InitDelayed(Transform t)
        {
            yield return new WaitForEndOfFrame();
            LevelDetailViewController.AttachTo(t.Find("LevelDetail"));
        }
    }
}
