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
        public TextMeshProUGUI rankPlateText;

        bool mapRanked;
        bool showBanButton;
        bool showSeedButton;

        public event PropertyChangedEventHandler PropertyChanged;

        private string _banHover;
        private string _likeHover;

        private string _banColor = "white";
        private string _likeColor = "white";

        private int _banDays;

        private string _rankPlateText;

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

        [UIValue("rankplate")]
        private string RankPlate
        {
            get => _rankPlateText;
            set
            {
                _rankPlateText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RankPlate)));
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

            BSMLParser.Instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "SmartSongSuggest.UI.Views.LevelDetailSuggestButtonsView.bsml"), t.gameObject, persController);
            persController.rootTransform.localScale *= 0.7f;
            persController.sldv = GameObject.FindObjectOfType<StandardLevelDetailViewController>();
            persController.sldv.didChangeContentEvent += persController.didChangeContent;
            persController.sldv.didChangeDifficultyBeatmapEvent += persController.didChangeDifficulty;

            persController.CheckButtons();
        }

        private void didChangeDifficulty(StandardLevelDetailViewController arg1)
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

        string levelHash => sldv != null ? Hashing.GetCustomLevelHash(sldv.beatmapLevel) : null;
        string levelDifficulty => sldv?.beatmapKey.difficulty.SerializedName();
        string levelCharacteristic => sldv?.beatmapKey.beatmapCharacteristic?.serializedName;

        //Something changed so lets update display components to fit for next rendering.
        internal void CheckButtons()
        {
            try
            {
               
                //Reset variables to not show the components until they are revalidated
                addToIgnoredBTN.gameObject.SetActive(false);
                addToLikedBTN.gameObject.SetActive(false);
                showSeedButton = false;
                showBanButton = false;
                rankPlateText.text = "";

                SongSuggestManager.toolBox.log?.WriteLine("Checking if a non Custom Map has been selected");
                //Check if we are ready to update (general checks)
                //Check song is selected
                if (sldv == null) return;
                if (sldv.beatmapLevel.hasPrecalculatedData) return;

                //Check SongSuggest has finished loading
                if (SongSuggestManager.toolBox == null) return;

                SongSuggestManager.toolBox.log?.WriteLine($"Checking if map is Ranked: {levelCharacteristic} {levelDifficulty} {levelHash}");
                mapRanked = SongSuggestManager.toolBox.songLibrary.HasAnySongCategory(levelHash, levelDifficulty);
                
                //Components update their values based on their state.
                DisplayBan();
                DisplaySeed();
                DisplayRankPlate();

                //Update components with new values.
                SharedCoroutineStarter.instance.StartCoroutine(SetActiveLate());
                rankPlateText.text = _rankPlateText;

            }
            catch (Exception e)
            {
                Plugin.Log.Error(e);
            }

        }

        private void DisplayRankPlate()
        {
            if (!mapRanked) return;

            SongSuggestManager.toolBox.log?.WriteLine("Getting Songrank");
            string songRank = $"{SongSuggestManager.toolBox.GetSongRanking(levelCharacteristic, levelDifficulty, levelHash)}";
            string maxRank = $"{SongSuggestManager.toolBox.GetSongRankingCount()}";
            string AP = $"{SongSuggestManager.toolBox.GetAPString(levelHash, levelDifficulty)}";

            string rankPlateString = "";
            if (songRank != "") rankPlateString = $"{rankPlateString} {songRank}/{maxRank}";
            if (AP != "") rankPlateString = $"{rankPlateString} {AP}";
            rankPlateString = rankPlateString.Trim();

            SongSuggestManager.toolBox.log?.WriteLine("Checking if RankPlate is active, and song got a Ranking");
            if (rankPlateString != "" && SettingsController.cfgInstance.ShowRankPlate)
            {
                SongSuggestManager.toolBox.log?.WriteLine("Setting Rank Plate Text");
                _rankPlateText = rankPlateString;
            }
        }

        private void DisplaySeed()
        {
            //Only show for ranked maps, others cannot be displayed
            if (!mapRanked) return;
            showSeedButton = SettingsController.cfgInstance.ShowSeedButton && SettingsController.cfgInstance.UseSeedSongs;

            if (SongSuggestManager.toolBox.songLiking.IsLiked(levelHash, levelDifficulty))
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

            if (SongSuggestManager.toolBox.songBanning.IsBanned(levelCharacteristic, levelDifficulty, levelHash))
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
        }

        void CheckBanState()
        {
            string diffLabel = sldv.beatmapKey.difficulty.SerializedName();
            string characteristic = sldv.beatmapKey.beatmapCharacteristic.serializedName;

            if (SongSuggestManager.toolBox.songBanning.IsBanned(characteristic,diffLabel,levelHash))
            {
                SongSuggestManager.toolBox.songBanning.LiftBan(characteristic, diffLabel, levelHash);
            } 
            else
            {
                parserParams.EmitEvent("open-modal");
            }

            CheckButtons();
        }

        void AddDifficultyBeatmapToIgnored(int days)
        {
            string diffLabel = sldv.beatmapKey.difficulty.SerializedName();
            string characteristic = sldv.beatmapKey.beatmapCharacteristic.serializedName;

            if (days == -1)
            {
                SongSuggestManager.toolBox.songBanning.SetPermaBan(characteristic, diffLabel ,levelHash);
            } 
            else
            {
                SongSuggestManager.toolBox.songBanning.SetBan(characteristic, diffLabel, levelHash, days);
            }

            CheckButtons();
        }

        void AddDifficultyBeatmapToLiked()
        {
            if (SongSuggestManager.toolBox.songLiking.IsLiked(levelHash, sldv.beatmapKey.difficulty.Name()))
            {
                SongSuggestManager.toolBox.songLiking.RemoveLike(levelHash, sldv.beatmapKey.difficulty.Name());
            } 
            else
            {
                SongSuggestManager.toolBox.songLiking.SetLike(levelHash, sldv.beatmapKey.difficulty.Name());
            }

            CheckButtons();

        }

        public virtual void RankPlateChanged()
        {
            CheckButtons();
        }
    }
}
