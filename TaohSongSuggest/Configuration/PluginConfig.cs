
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BeatSaberMarkupLanguage.Attributes;
using IPA.Config.Stores;
using UnityEngine;
using SongSuggestNS;
using System;
using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;
using Newtonsoft.Json;
using SmartSongSuggest.UI;
using System.ComponentModel;
using IPA.Config.Data;


[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace SmartSongSuggest.Configuration
{
    internal class PluginConfig : INotifyPropertyChanged
    {
        //DaysSliderMin: This variable represents the minimum value for the accuracy range set by the slider in your UI.
        //DaysSliderMax: This variable represents the maximum value for the accuracy range set by the slider in your UI.
        //DaysSliderIncrement: This variable represents the increment or step size for adjusting the accuracy range on the slider.
        //DaysUserSelectionMin: This variable represents the minimum value selected by the user within the accuracy range.
        //DaysUserSelectionMax: This variable represents the maximum value selected by the user within the accuracy range.



        //Min/Max Values for Sliders/Selections. Adjustable in Config.
        public virtual string __comment_Suggest__ { get; set; } = "Configureable Values for the Suggestions Tab starts here";
        public virtual int SuggestPlaylistMinCount { get; set; } = 10;
        public virtual int SuggestPlaylistMaxCount { get; set; } = 100;
        public virtual int SuggestIgnorePlayedDaysMaxCount { get; set; } = 100;

        public virtual string __comment_OldnNew__ { get; set; } = "Configureable Values for the Old & New Tab starts here";
        public virtual int OldnNewPlaylistMinCount { get; set; } = 10;
        public virtual int OldnNewPlaylistMaxCount { get; set; } = 100;
        [UIValue("acc-slider-min")]
        public virtual float AccSliderMin { get; set; } = 70f;
        [UIValue("acc-slider-max")]
        public virtual float AccSliderMax { get; set; } = 100f;
        [UIValue("acc-slider-increment")]
        public virtual float AccSliderIncrement { get; set; } = 0.1f;
        public virtual int AccSliderDecimals { get; set; } = 1;

        [UIValue("age-slider-min")]
        public virtual int AgeSliderMin { get; set; } = 5;
        [UIValue("age-slider-max")]
        public virtual int AgeSliderMax { get; set; } = 365;
        [UIValue("age-slider-increment")]
        public virtual int AgeSliderIncrement { get; set; } = 5;
        [UIValue("star-slider-increment")]
        public virtual float StarSliderIncrement { get; set; } = 0.1f;
        public virtual int StarSliderDecimals { get; set; } = 1;
        [UIValue("complexity-slider-increment")]
        public virtual float ComplexitySliderIncrement { get; set; } = 0.1f;
        public virtual int ComplexitySliderDecimals { get; set; } = 1;

        public virtual string __comment_Variables__ { get; set; } = "Remaining Values are in game selectable Values";

        //Stores if defaults been updated.
        public virtual bool HasResetDefaultValuesTo2_0 { get; set; } = false;

        //Storage of Active Selected Sliders Values and default selection Value (on new create).
        private float _accUserSelectionMin = 0;         //Old & New Min Sliders Accuracy Value (value set between OldnNewAccuracyMin and _highestAcc)
        private float _accUserSelectionMax = 100;       //Old & New Max Sliders Accuracy Value (value set between _lowestAcc and OldnNewAccuracyMax)
        private int _ageUserSelectionMin = 0;           //Old & New Slider Min Value Age of play
        private int _ageUserSelectionMax = int.MaxValue;//Old & New Slider Max Value Age of play
        private float _starUserSelectionMin = 0;        //Old & New Slider Min Star
        private float _starUserSelectionMax = 15;       //Old & New Slider Max Star
        private float _complexityUserSelectionMin = 0;  //Old & New Slider Min Complexity
        private float _complexityUserSelectionMax = 15; //Old & New Slider Max Complexity
        private bool _selectionBestWorst = false;
        private bool _orderBestWorst = true;
        private bool _useAcc = false;
        private bool _useAge = false;
        private bool _useStars = false;
        private bool _useComplexity = false;

        //Mutex to ensure slider updates does not cause infinite recall loops at certain increment values
        private bool sliderMutexFree = true;

        [UIValue("use_acc_inv")]
        public virtual bool useAccInv { get => !UseAcc; }
        [UIValue("use_age_inv")]
        public virtual bool useAgeInv { get => !UseAge; }
        [UIValue("use_stars_inv")]
        public virtual bool useStarsInv { get => !UseStars; }
        [UIValue("use_complexity_inv")]
        public virtual bool useComplexityInv { get => !UseComplexity; }

        //[UIValue("")]
        [UIValue("ignorePlayedDays")]
        public virtual int IgnorePlayedDays { get; set; } = 7;
        [UIValue("useLikedSongs")]
        public virtual bool UseLikedSongs { get; set; }
        [UIValue("fillLikedSongs")]
        public virtual bool FillLikedSongs { get; set; }
        [UIValue("modifierStyle")]
        public virtual int ModifierStyle { get; set; } = 100;
        [UIValue("modifierOverweight")]
        public virtual int ModifierOverweight { get; set; } = 81;
        [UIValue("removeOptimizedScores")]
        public virtual bool RemoveOptimizedScores { get; set; } = true;
        [UIValue("extraSongs")]
        public virtual int ExtraSongs { get; set; } = 75;

        [UIValue("showRankPlate")]
        public virtual bool ShowRankPlate { get; set; } = true;
        [UIValue("showLikeButton")]
        public virtual bool ShowLikeButton { get; set; } = false;
        [UIValue("showBanButton")]
        public virtual bool ShowBanButton { get; set; } = true;
        [UIValue("oldnNew_playlist_count")]
        public virtual int OldnNewPlaylistCount { get; set; } = 25;
        [UIValue("suggest_playlist_count")]
        public virtual int SuggestPlaylistCount { get; set; } = 50;
        [UIValue("weighted")]
        public virtual int RandomWeight { get; set; } = 30;

        //SuggestIgnorePlayedDaysMaxCount + 1
        [UIValue("ignoreAllPlayedDays")]
        public virtual int SuggestIgnorePlayedDaysAllCount 
        {
            get => SuggestIgnorePlayedDaysMaxCount + 1;
        }

        [UIValue("old_lowest_acc")]
        public virtual float AccUserSelectionMin
        {
            get => _accUserSelectionMin;
            set
            {
                _accUserSelectionMin = value;

                if (_accUserSelectionMin > AccUserSelectionMax - AccSliderIncrement && sliderMutexFree)
                {
                    sliderMutexFree = false;
                    AccUserSelectionMax = _accUserSelectionMin + AccSliderIncrement;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AccUserSelectionMax)));
                    sliderMutexFree = true;
                }
            }
        }

        [UIValue("old_highest_acc")]
        public virtual float AccUserSelectionMax
        {
            get => _accUserSelectionMax;
            set
            {
                _accUserSelectionMax = value;

                if (_accUserSelectionMax < AccUserSelectionMin + AccSliderIncrement && sliderMutexFree)
                {
                    sliderMutexFree = false;
                    AccUserSelectionMin = _accUserSelectionMax - AccSliderIncrement;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AccUserSelectionMin)));
                    sliderMutexFree = true;
                }
            }
        }

        [Ignore]
        [UIValue("acc-slider-abs-min")]
        public virtual float AccSliderAbsMin
        {
            get => AccSliderMin - AccSliderIncrement;
        }

        [Ignore]
        [UIValue("acc-slider-abs-max")]
        public virtual float AccSliderAbsMax
        {
            get => AccSliderMax + AccSliderIncrement;
        }

        [UIValue("old_oldest_days")]
        public virtual int AgeUserSelectionMin
        {
            get => _ageUserSelectionMin;
            set
            {
                _ageUserSelectionMin = value;

                if (_ageUserSelectionMin > AgeUserSelectionMax - AgeSliderIncrement && sliderMutexFree)
                {
                    sliderMutexFree = false;
                    AgeUserSelectionMax = _ageUserSelectionMin + AgeSliderIncrement;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AgeUserSelectionMax)));
                    sliderMutexFree = true;
                }
            }
        }

        [UIValue("old_newest_days")]
        public virtual int AgeUserSelectionMax
        {
            get => _ageUserSelectionMax;
            set
            {
                _ageUserSelectionMax = value;

                if (_ageUserSelectionMax < AgeUserSelectionMin + AgeSliderIncrement && sliderMutexFree)
                {
                    sliderMutexFree = false;
                    AgeUserSelectionMin = _ageUserSelectionMax - AgeSliderIncrement;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AgeUserSelectionMin)));
                    sliderMutexFree = true;
                }
            }
        }

        [UIValue("old_lowest_stars")]
        public virtual float StarUserSelectionMin
        {
            get => _starUserSelectionMin;
            set
            {
                _starUserSelectionMin = value;

                if (_starUserSelectionMin > StarUserSelectionMax - StarSliderIncrement && sliderMutexFree)
                {
                    sliderMutexFree = false;
                    StarUserSelectionMax = _starUserSelectionMin + StarSliderIncrement;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StarUserSelectionMax)));
                    sliderMutexFree = true;
                }
            }
        }

        [UIValue("old_highest_stars")]
        public virtual float StarUserSelectionMax
        {
            get => _starUserSelectionMax;
            set
            {
                _starUserSelectionMax = value;

                if (_starUserSelectionMax < StarUserSelectionMin + StarSliderIncrement && sliderMutexFree)
                {
                    sliderMutexFree = false;
                    StarUserSelectionMin = _starUserSelectionMax - StarSliderIncrement;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StarUserSelectionMin)));
                    sliderMutexFree = true;
                }
            }
        }

        [UIValue("old_lowest_complexity")]
        public virtual float ComplexityUserSelectionMin
        {
            get => _complexityUserSelectionMin;
            set
            {
                _complexityUserSelectionMin = value;

                if (_complexityUserSelectionMin > ComplexityUserSelectionMax - ComplexitySliderIncrement && sliderMutexFree)
                {
                    sliderMutexFree = false;
                    ComplexityUserSelectionMax = _complexityUserSelectionMin + ComplexitySliderIncrement;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ComplexityUserSelectionMax)));
                    sliderMutexFree = true;
                }
            }
        }

        [UIValue("old_highest_complexity")]
        public virtual float ComplexityUserSelectionMax
        {
            get => _complexityUserSelectionMax;
            set
            {
                _complexityUserSelectionMax = value;

                if (_complexityUserSelectionMax < ComplexityUserSelectionMin + ComplexitySliderIncrement && sliderMutexFree)
                {
                    sliderMutexFree = false;
                    ComplexityUserSelectionMin = _complexityUserSelectionMax - ComplexitySliderIncrement;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ComplexityUserSelectionMin)));
                    sliderMutexFree = true;
                }
            }
        }

        [Ignore]
        [UIValue("age-slider-abs-min")]
        public virtual float AgeSliderAbsMin
        {
            get => AgeSliderMin - AgeSliderIncrement;
        }

        [Ignore]
        [UIValue("age-slider-abs-max")]
        public virtual float AgeSliderAbsMax
        {
            get => AgeSliderMax + AgeSliderIncrement;
        }

        [Ignore]
        [UIValue("star-slider-max")]
        public float StarSliderMax
        {
            get
            {
                double star = SmartSongSuggest.Managers.SongSuggestManager.toolBox.songLibrary.songs.OrderByDescending(c => c.Value.starBeatSaber).First().Value.starBeatSaber;
                star = Math.Ceiling(star / StarSliderIncrement) * StarSliderIncrement - StarSliderIncrement;
                return (float)star;
            }
        }

        [Ignore]
        [UIValue("star-slider-abs-max")]
        public float StarSliderAbsMax
        {
            get
            {
                return StarSliderMax + StarSliderIncrement;
            }
        }


        [Ignore]
        [UIValue("complexity-slider-max")]
        public float ComplexitySliderMax
        {
            get
            {
                double star = SmartSongSuggest.Managers.SongSuggestManager.toolBox.songLibrary.songs.OrderByDescending(c => c.Value.complexityAccSaber).First().Value.complexityAccSaber;
                star = Math.Ceiling(star / ComplexitySliderIncrement) * ComplexitySliderIncrement - ComplexitySliderIncrement;
                return (float)star;
            }
        }

        [Ignore]
        [UIValue("complexity-slider-abs-max")]
        public float ComplexitySliderAbsMax
        {
            get
            {
                return ComplexitySliderMax + ComplexitySliderIncrement;
            }
        }

        private bool _showOrderOptions { get; set; } = false;
        
        [UIValue("showOrderOptions")]
        public virtual bool ShowOrderOptions
        {
            get => _showOrderOptions;
            set
            {
                _showOrderOptions = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowOrderOptions)));
            }
        }

        private bool _showLikedOptions { get; set; } = false;

        [UIValue("showLikedOptions")]
        public virtual bool ShowLikedOptions
        {
            get => _showLikedOptions;
            set
            {
                _showLikedOptions = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowLikedOptions)));
            }
        }

        [UIValue("use_acc")]
        public virtual bool UseAcc
        {
            get => _useAcc;
            set
            {
                _useAcc = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UseAcc)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(useAccInv)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AccColor)));
            }
        }

        [UIValue("use_age")]
        public virtual bool UseAge
        {
            get => _useAge;
            set
            {
                _useAge = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UseAge)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(useAgeInv)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AgeColor)));
            }
        }

        [UIValue("use_stars")]
        public virtual bool UseStars
        {
            get => _useStars;
            set
            {
                _useStars = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UseStars)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(useStarsInv)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StarsColor)));
            }
        }

        [UIValue("use_complexity")]
        public virtual bool UseComplexity
        {
            get => _useComplexity;
            set
            {
                _useComplexity = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UseComplexity)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(useComplexityInv)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ComplexityColor)));
            }
        }

        [UIValue("selectionBestWorst")]
        public virtual bool SelectionBestWorst { get => _selectionBestWorst; set { _selectionBestWorst = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectionOrder))); } }

        [UIValue("orderBestWorst")]
        public virtual bool OrderBestWorst { get => _orderBestWorst; set { _orderBestWorst = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OrderOrder))); } }

        //Assigns the first entry that is not .None to the default selections as well as generate the ordering list for the UI without the .None option

        [UIValue("oldest_selection")]
        public virtual SongSortCriteria PlaylistSelectionSort { get; set; } = Enum.GetValues(typeof(SongSortCriteria)).Cast<SongSortCriteria>().Skip(1).First();
        [UIValue("oldest_order")]
        public virtual SongSortCriteria PlaylistOrderingSort { get; set; } = Enum.GetValues(typeof(SongSortCriteria)).Cast<SongSortCriteria>().Skip(1).First();

        [Ignore]
        [UIValue("oldest_sorting")]
        //Skip the None option.
        public List<object> options = Enum.GetValues(typeof(SongSortCriteria)).Cast<object>().ToList().Skip(1).ToList();


        [NonNullable]
        [UseConverter(typeof(ListConverter<SongCategoryDisplay>))]
        public virtual List<SongCategoryDisplay> SongCategories { get; set; } = new List<SongCategoryDisplay>();

        [Ignore]
        [UIValue("selection-order")]
        public string SelectionOrder => SelectionBestWorst ? "Best to Worst" : "Worst to Best";

        [Ignore]
        [UIValue("order-order")]
        public string OrderOrder => OrderBestWorst ? "Best to Worst" : "Worst to Best";


        [Ignore]
        [UIValue("use-acc-color")]
        public string AccColor => UseAcc ? "white" : "grey";

        [Ignore]
        [UIValue("use-days-color")]
        public string AgeColor => UseAge ? "white" : "grey";

        [Ignore]
        [UIValue("use-stars-color")]
        public string StarsColor => UseStars ? "white" : "grey";

        [Ignore]
        [UIValue("use-complexity-color")]
        public string ComplexityColor => UseComplexity ? "white" : "grey";



        public event PropertyChangedEventHandler PropertyChanged;


        //Used by Suggest Sliders.
        [UIAction("percent-formatter")]
        public string PercentFormatter(float sliderVal)
        {
            return (Mathf.Round(sliderVal * 10) / 10).ToString("0.0") + "%";
        }

        [UIAction("percent-formatter-accuracy")]
        public string PercentFormatterAccuracy(float sliderVal)
        {
            //float val = Mathf.Round(sliderVal * 10) / 10;
            if (sliderVal < AccSliderMin)
                return "Any";
            if (sliderVal > AccSliderMax)
                return "All";

            String format = $"0.{new string('0', AccSliderDecimals)}";
            return $"{sliderVal.ToString(format)}%";
        }

        [UIAction("day-formatter")]
        public string DayFormatterAge(int sliderVal)
        {
            if (sliderVal < AgeSliderMin)
                return "Any";
            if (sliderVal > AgeSliderMax)
                return "All";

            return $"{sliderVal} Days";
        }

        [UIAction("star-formatter")]
        public string StarFormatter(float sliderVal)
        {
            if (sliderVal < StarSliderIncrement)
                return "Any";
            if (sliderVal > StarSliderMax)
                return "All";
            String format = $"0.{new string('0', StarSliderDecimals)}";
            return $"{sliderVal.ToString(format)}*";
        }

        [UIAction("complexity-formatter")]
        public string ComplexityFormatter(float sliderVal)
        {
            if (sliderVal < ComplexitySliderIncrement)
                return "Any";
            if (sliderVal > ComplexitySliderMax)
                return "All";
            String format = $"0.{new string('0', ComplexitySliderDecimals)}";
            return $"{sliderVal.ToString(format)}*";
        }

        [UIAction("percent-weighted")]
        public string PercentFormatterWeighted(float sliderVal)
        {
            if (sliderVal == 0) return $"Selection Sort Order";
            if (sliderVal == 100) return "True Random";
            return (Mathf.Round(sliderVal * 10) / 10).ToString("0.0") + "% Random";
        }

        [UIAction("day-ignorePlayed-formatter")]
        public string DayIgnorePlayedFormatter(int sliderVal)
        {
            if (sliderVal == SuggestIgnorePlayedDaysAllCount)
                return "Ignore All Played";

            return sliderVal + " Days";
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
            OrderBestWorst = !OrderBestWorst;
        }

        [UIAction("selection-order-click")]
        public void SelectionOrderClick()
        {
            SelectionBestWorst = !SelectionBestWorst;
        }

        public void ResetSettings()
        {
            CopyFrom(new PluginConfig()
            {
                ShowRankPlate = ShowRankPlate,
                ShowLikeButton = ShowLikeButton,
                ShowBanButton = ShowBanButton
            });

        }

        /// <summary>
        /// This is called whenever BSIPA reads the config from disk (including when file changes are detected).
        /// </summary>
        public virtual void OnReload()
        {
            // Do stuff after config is read from disk.

            //First run resets loaded values from previous versions to values is found optimal now.
            if (!HasResetDefaultValuesTo2_0)
            {
                HasResetDefaultValuesTo2_0 = true;
                ModifierOverweight = 81;
                ModifierStyle = 100;
            }

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
        public SongCategory SongCategory { get; set; }

        [JsonIgnore]
        [Ignore]
        [UIValue("title")]
        public string title { get => Managers.SongSuggestManager.toolBox.SongCategoryText($"{SongCategory}Label"); }

        [JsonIgnore]
        [Ignore]
        [UIValue("hover")]
        public string hover { get => Managers.SongSuggestManager.toolBox.SongCategoryText($"{SongCategory}Hover"); }

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
