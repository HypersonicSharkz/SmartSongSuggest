
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BeatSaberMarkupLanguage.Attributes;
using IPA.Config.Stores;
using UnityEngine;
using SongSuggestNS;
using System;
using IPA.Config.Stores.Attributes;
using System.Collections;
using IPA.Config.Stores.Converters;
using Newtonsoft.Json;
using SmartSongSuggest.UI;
using System.ComponentModel;
using System.Windows.Forms;
using IPA.Utilities;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace SmartSongSuggest.Configuration
{
    internal class PluginConfig : INotifyPropertyChanged
    {
        [Ignore]
        private float highestAcc = 100;
        private float lowestAcc = 0;
        private int oldestDays = 0;
        private int newestDays = int.MaxValue;
        private float lowestStars = 0;
        private float highestStars = 15;
        private float lowestComplexity = 0;
        private float highestComplexity = 15;
        private bool _selectionBestWorst = false;
        private bool _orderBestWorst = true;
        private bool _use_acc = false;
        private bool _use_days = false;
        private bool _use_stars = false;
        private bool _use_complexity = false;

        public virtual bool use_acc_inv { get => !use_acc; }
        public virtual bool use_days_inv { get => !use_days; }
        public virtual bool use_stars_inv { get => !use_stars; }
        public virtual bool use_complexity_inv { get => !use_complexity; }

        public virtual int fromRank { get; set; } = 1;
        public virtual int toRank { get; set; } = 10000;
        public virtual bool ignorePlayedAll { get; set; } = false;
        public virtual int ignorePlayedDays { get; set; } = 7;
        public virtual int ignoreAllPlayedDays { get; set; } = 101;
        public virtual int requiredMatches { get; set; } = 90;
        public virtual bool useLikedSongs { get; set; }
        public virtual bool fillLikedSongs { get; set; }
        public virtual int modifierPP { get; set; } = 20;
        public virtual int modifierStyle { get; set; } = 100;
        public virtual int modifierOverweight { get; set; } = 81;
        public virtual bool removeOptimizedScores { get; set; } = true;
        public virtual int extraSongs { get; set; } = 75;
        public virtual int skipSongsCount { get; set; } = 0;

        public virtual bool showRankPlate { get; set; } = true;
        public virtual bool showLikeButton { get; set; } = false;
        public virtual bool showBanButton { get; set; } = true;

        public virtual int old_playlist_count { get; set; } = 25;
        public virtual int suggest_playlist_count { get; set; } = 50;
        public virtual int weighted { get; set; } = 30;
        public virtual float old_highest_acc
        {
            get => highestAcc;
            set
            {
                highestAcc = value;

                if (highestAcc < old_lowest_acc)
                {
                    old_lowest_acc = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(old_lowest_acc)));
                }
            }
        }
        public virtual float old_lowest_acc
        {
            get => lowestAcc;
            set
            {
                lowestAcc = value;

                if (lowestAcc > old_highest_acc)
                {
                    old_highest_acc = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(old_highest_acc)));
                }
            }
        }

        public virtual int old_oldest_days
        {
            get => oldestDays;
            set
            {
                oldestDays = value;

                if (oldestDays > old_newest_days)
                {
                    old_newest_days = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(old_newest_days)));
                }
            }
        }

        public virtual int old_newest_days
        {
            get => newestDays;
            set
            {
                newestDays = value;

                if (newestDays < old_oldest_days)
                {
                    old_oldest_days = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(old_oldest_days)));
                }
            }
        }

        public virtual float old_lowest_stars
        {
            get => lowestStars;
            set
            {
                lowestStars = value;

                if (lowestStars > old_highest_stars)
                {
                    old_highest_stars = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(old_highest_stars)));
                }
            }
        }

        public virtual float old_highest_stars
        {
            get => highestStars;
            set
            {
                highestStars = value;

                if (highestStars < old_lowest_stars)
                {
                    old_lowest_stars = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(old_lowest_stars)));
                }
            }
        }

        public virtual float old_lowest_complexity
        {
            get => lowestComplexity;
            set
            {
                lowestComplexity = value;

                if (lowestComplexity > old_highest_complexity)
                {
                    old_highest_complexity = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(old_highest_complexity)));
                }
            }
        }
        public virtual float old_highest_complexity
        {
            get => highestComplexity;
            set
            {
                highestComplexity = value;

                if (highestComplexity < old_lowest_complexity)
                {
                    old_lowest_complexity = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(old_lowest_complexity)));
                }
            }
        }

        private bool _showOrderOptions { get; set; } = false;
        public virtual bool showOrderOptions
        {
            get => _showOrderOptions;
            set
            {
                _showOrderOptions = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(showOrderOptions)));
            }
        }

        private bool _showLikedOptions { get; set; } = false;

        public virtual bool showLikedOptions
        {
            get => _showLikedOptions;
            set
            {
                _showLikedOptions = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(showLikedOptions)));
            }
        }

        public virtual bool use_acc
        {
            get => _use_acc;
            set
            {
                _use_acc = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(use_acc)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(use_acc_inv)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AccColor)));
            }
        }
        public virtual bool use_days
        {
            get => _use_days;
            set
            {
                _use_days = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(use_days)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(use_days_inv)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DaysColor)));
            }
        }
        public virtual bool use_stars
        {
            get => _use_stars;
            set
            {
                _use_stars = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(use_stars)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(use_stars_inv)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StarsColor)));
            }
        }
        public virtual bool use_complexity
        {
            get => _use_complexity;
            set
            {
                _use_complexity = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(use_complexity)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(use_complexity_inv)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ComplexityColor)));
            }
        }

        public virtual bool selectionBestWorst { get => _selectionBestWorst; set { _selectionBestWorst = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectionOrder))); } }
        public virtual bool orderBestWorst { get => _orderBestWorst; set { _orderBestWorst = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OrderOrder))); } }
        
        //Assigns the first entry that is not .None to the default selections as well as generate the ordering list for the UI without the .None option
        public virtual SongSortCriteria oldest_selection { get; set; } = Enum.GetValues(typeof(SongSortCriteria)).Cast<SongSortCriteria>().Skip(1).First();
        public virtual SongSortCriteria oldest_order { get; set; } = Enum.GetValues(typeof(SongSortCriteria)).Cast<SongSortCriteria>().Skip(1).First();

        [Ignore]
        [UIValue("oldest_sorting")]
        //Skip the None option.
        public List<object> options = Enum.GetValues(typeof(SongSortCriteria)).Cast<object>().ToList().Skip(1).ToList();


        [NonNullable]
        [UseConverter(typeof(ListConverter<SongCategoryDisplay>))]
        public virtual List<SongCategoryDisplay> SongCategories { get; set; } = new List<SongCategoryDisplay>();

        [Ignore]
        [UIValue("max-star-rating")]
        public float maxStarRating
        {
            get
            {
                double star = SmartSongSuggest.Managers.SongSuggestManager.toolBox.songLibrary.songs.OrderByDescending(c => c.Value.starBeatSaber).First().Value.starBeatSaber;
                star = Mathf.Ceil((float)(star * 10)) / 10;
                return (float)star;
            }
        }

        [Ignore]
        [UIValue("max-complexity")]
        public float maxComplexity
        {
            get
            {
                double star = SmartSongSuggest.Managers.SongSuggestManager.toolBox.songLibrary.songs.OrderByDescending(c => c.Value.complexityAccSaber).First().Value.complexityAccSaber;
                star = Mathf.Ceil((float)(star * 10)) / 10;
                return (float)star;
            }
        }

        [Ignore]
        [UIValue("selection-order")]
        public string SelectionOrder => selectionBestWorst ? "Best to Worst" : "Worst to Best";

        [Ignore]
        [UIValue("order-order")]
        public string OrderOrder => orderBestWorst ? "Best to Worst" : "Worst to Best";


        [Ignore]
        [UIValue("use-acc-color")]
        public string AccColor => use_acc ? "white" : "grey";

        [Ignore]
        [UIValue("use-days-color")]
        public string DaysColor => use_days ? "white" : "grey";

        [Ignore]
        [UIValue("use-stars-color")]
        public string StarsColor => use_stars ? "white" : "grey";

        [Ignore]
        [UIValue("use-complexity-color")]
        public string ComplexityColor => use_complexity ? "white" : "grey";



        public event PropertyChangedEventHandler PropertyChanged;

        [UIAction("percent-formatter")]
        public string PercentFormatter(float sliderVal)
        {
            return (Mathf.Round(sliderVal * 10) / 10).ToString("0.0") + "%";
        }

        [UIAction("percent-formatter-above")]
        public string PercentFormatterAbove(float sliderVal)
        {
            float val = Mathf.Round(sliderVal * 10) / 10;
            if (val < 70f)
                return "All";

            return val.ToString("0.0") + "%";
        }

        [UIAction("percent-formatter-below")]
        public string PercentFormatterBelow(float sliderVal)
        {
            float val = Mathf.Round(sliderVal * 10) / 10;
            if (val < 70f)
                return "None";

            return val.ToString("0.0") + "%";
        }

        [UIAction("percent-weighted")]
        public string PercentFormatterWeighted(float sliderVal)
        {
            if (sliderVal == 0) return $"Selection Sort Order";
            if (sliderVal == 100) return "True Random";
            return (Mathf.Round(sliderVal * 10) / 10).ToString("0.0") + "% Random";
        }


        [UIAction("day-formatter")]
        public string DayFormatter(int sliderVal)
        {
            if (sliderVal > 365)
                return "Any";

            return sliderVal + " Days";
        }

        [UIAction("day-ignorePlayed-formatter")]
        public string DayIgnorePlayedFormatter(int sliderVal)
        {
            if (sliderVal == ignoreAllPlayedDays)
                return "Ignore All Played";

            return sliderVal + " Days";
        }

        [UIAction("star-formatter")]
        public string StarFormatter(float sliderVal)
        {
            float val = Mathf.Round(sliderVal * 10) / 10;
            return val + " ⭐";
        }

        [UIAction("song-formatter")]
        public string SongFormatter(int sliderVal)
        {
            if (sliderVal == 0) return sliderVal + " Song";
            return sliderVal + " Songs";
        }

        [UIAction("order-order-click")]
        public void OrderOrderClick()
        {
            orderBestWorst = !orderBestWorst;
        }

        [UIAction("selection-order-click")]
        public void SelectionOrderClick()
        {
            selectionBestWorst = !selectionBestWorst;
        }

        public void ResetSettings()
        {
            CopyFrom(new PluginConfig()
            {
                showRankPlate = showRankPlate,
                showLikeButton = showLikeButton,
                showBanButton = showBanButton
            });

        }

        /// <summary>
        /// This is called whenever BSIPA reads the config from disk (including when file changes are detected).
        /// </summary>
        public virtual void OnReload()
        {
            // Do stuff after config is read from disk.
        }

        /// <summary>
        /// Call this to force BSIPA to update the config file. This is also called by BSIPA if it detects the file was modified.
        /// </summary>
        public virtual void Changed()
        {
            // Do stuff when the config is changed.

        }

        /// <summary>
        /// Call this to have BSIPA copy the values from <paramref name="other"/> into this config.
        /// </summary>
        public virtual void CopyFrom(PluginConfig other)
        {
            // This instance's members populated from other
        }
    }

    [JsonObject(MemberSerialization.Fields)]
    public class SongCategoryDisplay
    {
        public SongCategory songCategory { get; set; }

        [JsonIgnore]
        [Ignore]
        [UIValue("title")]
        public string title { get => Managers.SongSuggestManager.toolBox.SongCategoryText($"{songCategory}Label"); }

        [JsonIgnore]
        [Ignore]
        [UIValue("hover")]
        public string hover { get => Managers.SongSuggestManager.toolBox.SongCategoryText($"{songCategory}Hover"); }

        [UIValue("played")]
        public bool Played { get; set; }

        [UIValue("unplayed")]
        public bool Unplayed { get; set; }

        [UIAction("onchange")]
        public void OnChange(bool check)
        {
            SettingsController.cfgInstance.Changed();
        }
    }
}
