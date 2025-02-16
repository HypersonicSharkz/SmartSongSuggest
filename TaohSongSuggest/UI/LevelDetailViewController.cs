using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using System.Reflection;
using UnityEngine;
using System.ComponentModel;
using System;
using HMUI;
using SongDetailsCache.Structs;
using SongCore.Utilities;
using SmartSongSuggest.Managers;
using TMPro;
using System.Collections;
using BeatSaberMarkupLanguage.Parser;
using SongSuggestNS;
using SmartSongSuggest.Configuration;
using BeatSaberPlaylistsLib;
using SongLibraryNS;

namespace SmartSongSuggest.UI
{
    class LevelDetailViewController : INotifyPropertyChanged
    {
        internal static readonly LevelDetailViewController persController = new LevelDetailViewController();
        internal StandardLevelDetailViewController sldv;

        [UIParams]
        private readonly BSMLParserParams parserParams;

        [UIComponent("root")]
        private readonly Transform rootTransform;

        [UIComponent("addToIgnoredBTN")]
        private readonly NoTransitionsButton addToIgnoredBTN;

        [UIComponent("addToLikedBTN")]
        private readonly NoTransitionsButton addToLikedBTN;

        [UIComponent("addToIgnoredBTN")]
        public TextMeshProUGUI addToIgnoredBTNText;

        [UIComponent("addToLikedBTN")]
        public TextMeshProUGUI addToLikedBTNText;

        [UIComponent("rankPlateText")]
        public TextMeshProUGUI rankPlateTextMesh;

        bool mapRanked;
        bool showRankPlate;
        bool showBanButton;
        bool showSeedButton;

        public event PropertyChangedEventHandler PropertyChanged;

        private string _banHover;
        private string _likeHover;

        private string _banColor = "white";
        private string _likeColor = "white";

        private int _banDays;

        [UIValue("ban-hover")]
        private string BanHover
        {
            get => _banHover;
            set
            {
                _banHover = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BanHover)));
            }
        }

        [UIValue("like-hover")]
        private string LikeHover
        {
            get => _likeHover;
            set
            {
                _likeHover = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LikeHover)));
            }
        }

        [UIValue("ban-color")]
        private string BanColor
        {
            get => _banColor;
            set
            {
                _banColor = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BanColor)));
            }
        }

        [UIValue("like-color")]
        private string LikeColor
        {
            get => _likeColor;
            set
            {
                _likeColor = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LikeColor)));
            }
        }

        [UIValue("ban-short")]
        private int BanShort => SettingsController.cfgInstance.BanShort; 

        [UIValue("ban-medium")]
        private int BanMedium => SettingsController.cfgInstance.BanMedium;
        [UIValue("ban-long")]
        private int BanLong => SettingsController.cfgInstance.BanLong;

        [UIValue("ban-length")]
        private string BanCustom
        {
            get => SettingsController.cfgInstance.BanCustom.ToString();
            set
            {
                if (int.TryParse(value, out _banDays))
                {
                    SettingsController.cfgInstance.BanCustom = _banDays;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BanCustom)));
                }
            }
        }

        [UIAction("one-day-ban")]
        private void odb() => AddDifficultyBeatmapToIgnored(SettingsController.cfgInstance.BanShort);

        [UIAction("one-week-ban")]
        private void owb() => AddDifficultyBeatmapToIgnored(SettingsController.cfgInstance.BanMedium);

        [UIAction("one-month-ban")]
        private void omb() => AddDifficultyBeatmapToIgnored(SettingsController.cfgInstance.BanLong);

        [UIAction("perm-ban")]
        private void pb() => AddDifficultyBeatmapToIgnored(-1);


        [UIAction("custom-day-ban")]
        private void CustomDayBan()
        {
            AddDifficultyBeatmapToIgnored(SettingsController.cfgInstance.BanCustom);
        }

        LevelDetailViewController() { }

        public static void AttachTo(Transform t)
        {
            if (t == null)
                return;

            BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "SmartSongSuggest.UI.Views.LevelDetailSuggestButtonsView.bsml"), t.gameObject, persController);
            persController.rootTransform.localScale *= 0.7f;
            persController.sldv = GameObject.FindObjectOfType<StandardLevelDetailViewController>();
            persController.sldv.didChangeContentEvent += persController.didChangeContent;
            persController.sldv.didChangeDifficultyBeatmapEvent += persController.didChangeDifficulty;

            persController.CheckButtons();
        }

        private void didChangeDifficulty(StandardLevelDetailViewController arg1,IDifficultyBeatmap arg2)
        {
            CheckButtons();
        }

        private void didChangeContent(StandardLevelDetailViewController arg1, StandardLevelDetailViewController.ContentType arg2)
        {
            if (arg2 == StandardLevelDetailViewController.ContentType.OwnedAndReady)
            { 
                CheckButtons();
            } 
        }

        string levelHash => sldv != null ? Hashing.GetCustomLevelHash(sldv.beatmapLevel is CustomBeatmapLevel custom? custom : null) : null;
        string levelDifficulty => sldv?.selectedDifficultyBeatmap.difficulty.SerializedName();
        string levelCharacteristic => sldv?.selectedDifficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic?.serializedName;
        SongID songID => SongSuggestManager.toolBox.songLibrary.GetID(levelCharacteristic, levelDifficulty, levelHash);


        //Something changed so lets update display components to fit for next rendering.
        internal void CheckButtons()
        {
            try
            {
               
                //Reset variables to not show the components until they are revalidated
                addToIgnoredBTN.gameObject.SetActive(false);
                addToLikedBTN.gameObject.SetActive(false);
                rankPlateTextMesh.gameObject.SetActive(false);
                showSeedButton = false;
                showBanButton = false;
                showRankPlate = false;

                SongSuggestManager.toolBox.log?.WriteLine("Checking if a non Custom Map has been selected");
                //Check if we are ready to update (general checks)
                //Check song is selected
                if (sldv == null) return;
                if (!(sldv.beatmapLevel is CustomBeatmapLevel)) return;
                //if (sldv.beatmapLevel.hasPrecalculatedData) return;

                //Check SongSuggest has finished loading
                if (SongSuggestManager.toolBox == null) return;

                SongSuggestManager.toolBox.log?.WriteLine($"Checking if map is Ranked: {levelCharacteristic} {levelDifficulty} {levelHash}");
                mapRanked = SongSuggestManager.toolBox.songLibrary.HasAnySongCategory(songID);
                
                //Components update their values based on their state.
                DisplayBan();
                DisplaySeed();
                DisplayRankPlate();

                //Update components with new values.
                SharedCoroutineStarter.instance.StartCoroutine(SetActiveLate());


            }
            catch (Exception e)
            {
                Plugin.Log.Error(e);
            }

        }

        private void DisplayRankPlate()
        {
            //Reset text for now
            rankPlateTextMesh.text = "";

            SongSuggestManager.toolBox.log?.WriteLine($"Checking if RankPlate is active: {SettingsController.cfgInstance.ShowRankPlate}, and song got a Ranking: {mapRanked}");

            if (!mapRanked) return;
            showRankPlate = SettingsController.cfgInstance.ShowRankPlate;

            SongSuggestManager.toolBox.log?.WriteLine("Getting Songrank");
            string songRank = $"{SongSuggestManager.toolBox.GetSongRanking(songID)}";
            string maxRank = $"{SongSuggestManager.toolBox.GetSongRankingCount()}";
            string AP = $"{SongSuggestManager.toolBox.GetAPString(songID)}";

            string rankPlateString = "";
            if (songRank != "") rankPlateString = $"{rankPlateString} {songRank}/{maxRank}";
            if (AP != "") rankPlateString = $"{rankPlateString} {AP}";
            rankPlateString = rankPlateString.Trim();

            SongSuggestManager.toolBox.log?.WriteLine($"Setting Rank Plate Text: {rankPlateString}");
            rankPlateTextMesh.text = rankPlateString;
        }

        private void DisplaySeed()
        {
            //Only show for ranked maps, others cannot be displayed
            if (!mapRanked) return;
            showSeedButton = SettingsController.cfgInstance.ShowSeedButton && SettingsController.cfgInstance.UseSeedSongs;

            if (SongSuggestManager.toolBox.songLiking.IsLiked(songID))
            {
                LikeHover = "Remove song from seed songs.";
                LikeColor = "green";
            }
            else
            {
                LikeHover = "Mark as a seed song.";
                LikeColor = "white";
            }
        }

        private void DisplayBan()
        {
            showBanButton = SettingsController.cfgInstance.ShowBanButton;

            if (SongSuggestManager.toolBox.songBanning.IsBanned(songID))
            {
                BanHover = "Click to unban map from suggestions";
                BanColor = "red";
            }
            else
            {
                BanHover = "Don't want to see this map in your suggestions? Press this";
                BanColor = "white";

            }
        }

        IEnumerator SetActiveLate()
        {
            yield return new WaitForEndOfFrame();
            addToIgnoredBTN.gameObject.SetActive(showBanButton);
            addToLikedBTN.gameObject.SetActive(showSeedButton);
            rankPlateTextMesh.gameObject.SetActive(showRankPlate);
        }

        void CheckBanState()
        {
            //string diffLabel = sldv.beatmapKey.difficulty.SerializedName();
            //string characteristic = sldv.beatmapKey.beatmapCharacteristic.serializedName;

            if (SongSuggestManager.toolBox.songBanning.IsBanned(songID))
            {
                SongSuggestManager.toolBox.songBanning.LiftBan(songID);
            } 
            else
            {
                parserParams.EmitEvent("open-modal");
            }

            CheckButtons();
        }

        void AddDifficultyBeatmapToIgnored(int days)
        {
            //string diffLabel = sldv.beatmapKey.difficulty.SerializedName();
            //string characteristic = sldv.beatmapKey.beatmapCharacteristic.serializedName;

            if (days == -1)
            {
                SongSuggestManager.toolBox.songBanning.SetPermaBan(songID);
            } 
            else
            {
                SongSuggestManager.toolBox.songBanning.SetBan(songID, days);
            }

            CheckButtons();
        }

        void AddDifficultyBeatmapToLiked()
        {
            if (SongSuggestManager.toolBox.songLiking.IsLiked(songID))
            {
                SongSuggestManager.toolBox.songLiking.RemoveLike(songID);
            } 
            else
            {
                SongSuggestManager.toolBox.songLiking.SetLike(songID);
            }

            CheckButtons();

        }

        public virtual void RankPlateChanged()
        {
            CheckButtons();
        }
    }
}
