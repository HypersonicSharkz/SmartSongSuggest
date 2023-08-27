﻿using System;
using Newtonsoft.Json;
using SongSuggestNS;


namespace SongLibraryNS
{
    public class Song 
    {
        public String scoreSaberID { get; set; }
        public String name { get; set; }
        public String hash { get; set; }
        public String difficulty { get; set; }
        public SongCategory songCategory { get; set; }
        public double starBeatSaber { get; set; }
        public double complexityAccSaber { get; set; }

        public Song()
        {
        }

        public String GetDifficultyText()
        {
            switch (difficulty)
            {
                case "1":
                    return "Easy";
                case "3":
                    return "Normal";
                case "5":
                    return "Hard";
                case "7":
                    return "Expert";
                case "9":
                    return "ExpertPlus";
                default:
                    return "Easy";
            }
        }
    }
}
