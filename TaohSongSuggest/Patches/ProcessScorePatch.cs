﻿using HarmonyLib;
using SmartSongSuggest.UI;

namespace SmartSongSuggest.Patches
{
    [HarmonyPatch(typeof(LevelCompletionResultsHelper), nameof(LevelCompletionResultsHelper.ProcessScore))]
    static class ProcessScorePatch
    {
        static void Postfix(PlayerData playerData, PlayerLevelStatsData playerLevelStats, LevelCompletionResults levelCompletionResults, IReadonlyBeatmapData transformedBeatmapData, IDifficultyBeatmap difficultyBeatmap, PlatformLeaderboardsModel platformLeaderboardsModel)
        {
            if (!SettingsController.cfgInstance.RecordLocalScores) return;
            if (BS_Utils.Gameplay.ScoreSubmission.Disabled) return;
            if (levelCompletionResults.levelEndStateType == LevelCompletionResults.LevelEndStateType.Failed) return;

            string mapType = playerLevelStats.beatmapCharacteristic.serializedName;
            if (mapType.ToLowerInvariant() != "standard") return;

            float maxScore = ScoreModel.ComputeMaxMultipliedScoreForBeatmap(transformedBeatmapData);
            int modifiedScore = levelCompletionResults.modifiedScore;
            int multipliedScore = levelCompletionResults.multipliedScore;

            if (modifiedScore == 0 || maxScore == 0) return;

            if (modifiedScore > multipliedScore) return;

            float acc = modifiedScore / maxScore;
            string mapId = difficultyBeatmap.level.levelID.Substring(13).Split('_')[0];
            string difficulty = difficultyBeatmap.difficulty.SerializedName();

            Managers.SongSuggestManager.toolBox.AddLocalScore(mapId, difficulty, acc);
            LevelDetailViewController.persController.RankPlateChanged();
        }
    }
}