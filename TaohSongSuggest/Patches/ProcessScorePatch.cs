using HarmonyLib;
using SmartSongSuggest.UI;
using SongLibraryNS;

namespace SmartSongSuggest.Patches
{
    [HarmonyPatch(typeof(LevelCompletionResultsHelper), nameof(LevelCompletionResultsHelper.ProcessScore))]
    static class ProcessScorePatch
    {
        static void Postfix(in BeatmapKey beatmapKey, PlayerData playerData, PlayerLevelStatsData playerLevelStats, LevelCompletionResults levelCompletionResults, IReadonlyBeatmapData transformedBeatmapData, PlatformLeaderboardsModel platformLeaderboardsModel)
        {
            Managers.SongSuggestManager.toolBox.log?.WriteLine($"Processing Result Screen");
            //if (!SettingsController.cfgInstance.RecordLocalScores) return;
            
            //We respect score submissions being turned off
            if (BS_Utils.Gameplay.ScoreSubmission.Disabled) return;
            
            //A fail is a fail
            if (levelCompletionResults.levelEndStateType == LevelCompletionResults.LevelEndStateType.Failed) return;

            float maxScore = ScoreModel.ComputeMaxMultipliedScoreForBeatmap(transformedBeatmapData);
            int modifiedScore = levelCompletionResults.modifiedScore;
            int multipliedScore = levelCompletionResults.multipliedScore;

            //Avoid Lightshow maps with 0 notes, as it can give divice by 0 for acc. One should not record lightshows, one should enjoy them.
            if (modifiedScore == 0 || maxScore == 0) return;

            //Played hit 0 energy and failed during gameplay. This is leaderboard specific handling, for Song Suggest locals we only use songs that was completed without triggering NoFail.
            if (levelCompletionResults.energy == 0) return;
            
            //??Is this a check for speed FS/SF, or level failed with NoFail turned on??            
            //if (modifiedScore > multipliedScore) return;

            //Multiplied is "base score" and "modified score" is including modifiers, we record base acc, and further modifications should be done "elsewhere".
            float acc = multipliedScore / maxScore;

            string mapType = playerLevelStats.beatmapCharacteristic.serializedName;
            string mapId = beatmapKey.levelId.Substring(13).Split('_')[0];
            string difficulty = beatmapKey.difficulty.SerializedName();
            var songID = SongLibrary.GetID(mapType, difficulty, mapId);

            //Mod can only calculate with Normal speed for now.
            string speed = $"{levelCompletionResults.gameplayModifiers.songSpeed}";
            if (speed != "Normal") return;

            //**Missing ... generation of the modifiers text
            //Store Session Score
            Managers.SongSuggestManager.toolBox.AddSessionScore(songID, acc, "");

            //Verify that recording is active)
            if (SettingsController.cfgInstance.RecordLocalScores) Managers.SongSuggestManager.toolBox.AddLocalScore(songID, acc, "");
            
            //Update the RankPlate as data changed.
            LevelDetailViewController.persController.RankPlateChanged();
        }
    }
}
