using System;
using System.Collections.Generic;
using SongSuggestNS;

namespace LinkedData
{
    public class Top10kPlayers
    {
        public SongSuggest songSuggest {get;set;}
        public List<Top10kPlayer> top10kPlayers = new List<Top10kPlayer>();
        public SortedDictionary<String,Top10kSongMeta> top10kSongMeta = new SortedDictionary<String,Top10kSongMeta>();

        public void Save()
        {
            songSuggest.fileHandler.SaveLinkedData(top10kPlayers);
        }

        public void Load()
        {
            top10kPlayers = songSuggest.fileHandler.LoadLinkedData();
            GenerateTop10kSongMeta();
        }

        public void GenerateTop10kSongMeta()
        {
            foreach (Top10kPlayer player in top10kPlayers)
            {
                foreach (Top10kScore score in player.top10kScore)
                {
                    //Add any missing songs.
                    if(!top10kSongMeta.ContainsKey(score.songID))
                    {
                        top10kSongMeta.Add(score.songID, new Top10kSongMeta{songID = score.songID});
                    }
                    Top10kSongMeta songMeta = top10kSongMeta[score.songID];
                    songMeta.count++;
                    songMeta.totalRank += score.rank;
                    songMeta.maxScore = Math.Max(songMeta.maxScore, score.pp);
                    songMeta.totalScore += score.pp;
                }
            }

            //set average for localvsglobal PP values
            foreach (Top10kSongMeta songMeta in top10kSongMeta.Values)
            {
                songMeta.averageScore = songMeta.totalScore / songMeta.count;
            }
            Console.WriteLine("*Total Songs*: " + top10kSongMeta.Count);
        }

        public void Add(String id, String name, int rank)
        {
            Top10kPlayer newPlayer = new Top10kPlayer();
            newPlayer.id = id;
            newPlayer.name = name;
            newPlayer.rank = rank;
            top10kPlayers.Add(newPlayer);
        }
    }
}