﻿using HarmonyLib;
using System.Collections;
using SmartSongSuggest.UI;
using UnityEngine;
using SmartSongSuggest.Managers;
using System;
using BeatSaberPlaylistsLib;

namespace SmartSongSuggest.Patches
{
    [HarmonyPatch(typeof(StandardLevelDetailViewController), "DidActivate")]
    static class LevelDetailPatch
    {
        static void Prefix(StandardLevelDetailViewController __instance, bool firstActivation)
        {
            //Informs the SongSuggestManager a new assignment is needed.
            if (firstActivation)
            {
                if (SettingsController.cfgInstance.LogEnabled) Console.WriteLine("Prefix: First Activation");
                SongSuggestManager.needsAssignment = true;
            }

            //Only activate once on a new reload, and only after SongSuggestCore is done loading
            if (SongSuggestManager.needsAssignment)
            {
                SongSuggestManager.needsAssignment = false;
                SharedCoroutineStarter.instance.StartCoroutine(InitDelayed(__instance.transform));
                SettingsController.cfgInstance.CachedPlayerID = BS_Utils.Gameplay.GetUserInfo.GetUserID();
            }
        }

        static IEnumerator InitDelayed(Transform t)
        {
            while (!SongSuggestManager.readyForAssignment)
                yield return new WaitForEndOfFrame();

            try
            {
                LevelDetailViewController.AttachTo(t.Find("LevelDetail"));
            }
            catch (Exception e)
            {
                SongSuggestManager.needsAssignment = true;
            }
        }
    }
}
