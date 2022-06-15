
using System.Runtime.CompilerServices;
using BeatSaberMarkupLanguage.Attributes;
using IPA.Config.Stores;
using UnityEngine;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace SmartSongSuggest.Configuration
{
    internal class PluginConfig
    {
        public virtual int fromRank { get; set; } = 1;
        public virtual int toRank { get; set; } = 10000;
        public virtual bool ignorePlayedAll { get; set; } = false;
        public virtual int ignorePlayedDays { get; set; } = 30;
        public virtual int requiredMatches { get; set; } = 90;
        public virtual bool useLikedSongs { get; set; }
        public virtual bool fillLikedSongs { get; set; }
        public virtual int modifierPP { get; set; } = 20;
        public virtual int modifierStyle { get; set; } = 100;
        public virtual int modifierOverweight { get; set; } = 60;
        public virtual bool removeOptimizedScores { get; set; } = true;

        public virtual bool showRankPlate { get; set; } = true;
        public virtual bool showLikeButton { get; set; } = true;
        public virtual bool showBanButton { get; set; } = true;


        public virtual float old_highest_acc { get; set; } = 100;
        public virtual int old_oldest_days { get; set; }


        [UIAction("percent-formatter")]
        public string PercentFormatter(float sliderVal)
        {
            return (Mathf.Round(sliderVal * 10) / 10).ToString("0.0") + "%";
        }

        [UIAction("day-formatter")]
        public string DayFormatter(int sliderVal)
        {
            return sliderVal + " Days";
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
}
