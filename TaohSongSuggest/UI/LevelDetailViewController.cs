using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using System.Reflection;
using UnityEngine;
using System.ComponentModel;
using System;
using HMUI;
using SongDetailsCache.Structs;
using SongDetailsCache;
using SongCore.Utilities;
using SmartSongSuggest.Managers;
using TMPro;
using BeatSaberMarkupLanguage.ViewControllers;
using System.Collections;
using BeatSaberMarkupLanguage.Parser;

namespace SmartSongSuggest.UI
{
    class LevelDetailViewController : INotifyPropertyChanged
    {
        internal static readonly LevelDetailViewController persController = new LevelDetailViewController();
        internal static bool HasAttached;
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

        bool _ignoreActive = true;
        bool _likeActive = true;

        public event PropertyChangedEventHandler PropertyChanged;


        private string _banHover;
        private string _likeHover;

        private string _banColor;
        private string _likeColor;

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

        [UIValue("ban-length")]
        private string BanDays
        {
            get => _banDays.ToString();
            set
            {
                if (!int.TryParse(value, out _banDays))
                    _banDays = 30;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BanDays)));
            }
        }




        [UIAction("one-day-ban")]
        private void odb() => AddDifficultyBeatmapToIgnored(1);

        [UIAction("one-week-ban")]
        private void owb() => AddDifficultyBeatmapToIgnored(7);

        [UIAction("one-month-ban")]
        private void omb() => AddDifficultyBeatmapToIgnored(30);

        [UIAction("perm-ban")]
        private void pb() => AddDifficultyBeatmapToIgnored(-1);


        [UIAction("custom-day-ban")]
        private void CustomDayBan()
        {
            AddDifficultyBeatmapToIgnored(_banDays);
        }

        LevelDetailViewController() { }

        public static void AttachTo(Transform t)
        {
            if (t == null)
                return;

            BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "SmartSongSuggest.UI.Views.LevelDetailSuggestButtonsView.bsml"), t.gameObject, persController);
            persController.rootTransform.localScale *= 0.7f;
            persController.sldv = GameObject.FindObjectOfType<StandardLevelDetailViewController>();
            persController.sldv.didChangeContentEvent += persController.didChangeContent;
            persController.sldv.didChangeDifficultyBeatmapEvent += persController.didChangeDifficulty;
            HasAttached = true;
        }

        private void didChangeDifficulty(StandardLevelDetailViewController arg1, IDifficultyBeatmap arg2)
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

        void CheckButtons()
        {


            BanDays = "60";

            _ignoreActive = false;
            _likeActive = false;

            addToIgnoredBTN.gameObject.SetActive(false);
            addToLikedBTN.gameObject.SetActive(false);



            Song x;
            if (sldv.beatmapLevel is CustomBeatmapLevel && Plugin.songDetails.songs.FindByHash(Hashing.GetCustomLevelHash(sldv.beatmapLevel as CustomBeatmapLevel), out x))
            {
                SongDifficulty difficulty;
                x.GetDifficulty(out difficulty, (MapDifficulty)sldv.selectedDifficultyBeatmap.difficulty); 

                _ignoreActive = difficulty.ranked;
                _likeActive = difficulty.ranked;

                //Forgot to check if map was ranked before checking ban... oops
                if (difficulty.ranked)
                {
                    if (SongSuggestManager.toolBox.songBanning.IsBanned(Hashing.GetCustomLevelHash(sldv.beatmapLevel as CustomBeatmapLevel), sldv.selectedDifficultyBeatmap.difficulty.Name()))
                    {
                        Console.WriteLine("Song is banned!");
                        BanHover = "Click to unban map from suggestions";
                        BanColor = "red";
                    }
                    else
                    {
                        BanHover = "Don't want to see this map in your suggestions? Press this";
                        BanColor = "white";

                    }


                    if (SongSuggestManager.toolBox.songLiking.IsLiked(Hashing.GetCustomLevelHash(sldv.beatmapLevel as CustomBeatmapLevel), sldv.selectedDifficultyBeatmap.difficulty.Name()))
                    {
                        Console.WriteLine("Song is liked!");
                        LikeHover = "Click to remove the map from songs the suggestions are based on";
                        LikeColor = "green";
                    }
                    else
                    {
                        LikeHover = "Want more maps like this in the suggestions? Press this";
                        LikeColor = "white";
                    }
                }

                SharedCoroutineStarter.instance.StartCoroutine(SetActiveLate());
            } 

        }

        IEnumerator SetActiveLate()
        {
            yield return new WaitForEndOfFrame();
            addToIgnoredBTN.gameObject.SetActive(_ignoreActive);
            addToLikedBTN.gameObject.SetActive(_likeActive);
        }

        void CheckBanState()
        {
            if (SongSuggestManager.toolBox.songBanning.IsBanned(Hashing.GetCustomLevelHash(sldv.beatmapLevel as CustomBeatmapLevel), sldv.selectedDifficultyBeatmap.difficulty.Name()))
            {
                SongSuggestManager.toolBox.songBanning.LiftBan(Hashing.GetCustomLevelHash(sldv.beatmapLevel as CustomBeatmapLevel), sldv.selectedDifficultyBeatmap.difficulty.Name());
            } else
            {
                parserParams.EmitEvent("open-modal");
            }

            CheckButtons();
        }

        void AddDifficultyBeatmapToIgnored(int days)
        {
            if (days == -1)
            {
                SongSuggestManager.toolBox.songBanning.SetPermaBan(Hashing.GetCustomLevelHash(sldv.beatmapLevel as CustomBeatmapLevel), sldv.selectedDifficultyBeatmap.difficulty.Name());
            } else
            {
                SongSuggestManager.toolBox.songBanning.SetBan(Hashing.GetCustomLevelHash(sldv.beatmapLevel as CustomBeatmapLevel), sldv.selectedDifficultyBeatmap.difficulty.Name(), days);
            }

            CheckButtons();
        }

        void AddDifficultyBeatmapToLiked()
        {
            if (SongSuggestManager.toolBox.songLiking.IsLiked(Hashing.GetCustomLevelHash(sldv.beatmapLevel as CustomBeatmapLevel), sldv.selectedDifficultyBeatmap.difficulty.Name()))
            {
                SongSuggestManager.toolBox.songLiking.RemoveLike(Hashing.GetCustomLevelHash(sldv.beatmapLevel as CustomBeatmapLevel), sldv.selectedDifficultyBeatmap.difficulty.Name());
            } else
            {

                SongSuggestManager.toolBox.songLiking.SetLike(Hashing.GetCustomLevelHash(sldv.beatmapLevel as CustomBeatmapLevel), sldv.selectedDifficultyBeatmap.difficulty.Name());
            }

            CheckButtons();

        }
    }
}
