using System;
using System.Net;
using ScoreSabersJson;
using Newtonsoft.Json;

namespace WebDownloading
{
    public class WebDownloader
    {
        public WebClient client = new WebClient();
        public JsonSerializerSettings serializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
        //Generic web puller for scores
        public PlayerScoreCollection GetScores(String id, String sorting, int count, int page)
        {
            try
            {
                //Console.WriteLine("Starting page " + page);

                //https://scoresaber.com/api/player/76561197993806676/scores?limit=20&sort=recent&page=2
                String scoresJSON = client.DownloadString("https://scoresaber.com/api/player/" + id + "/scores?limit=" + count + "&sort=" + sorting + "&page=" + page);
                return JsonConvert.DeserializeObject<PlayerScoreCollection>(scoresJSON, serializerSettings);
            }
            catch
            {
                Console.WriteLine("Error on " + page);
            }
            return new PlayerScoreCollection();
        }

        //Generic web puller for top players
        public PlayerCollection GetPlayers(int page)
        {
            try
            {
                Console.WriteLine("Starting page " + page);

                //https://scoresaber.com/api/players?page=2
                String playersJSON = client.DownloadString("https://scoresaber.com/api/players?page=" + page);
                return JsonConvert.DeserializeObject<PlayerCollection>(playersJSON, serializerSettings);

            }
            catch
            {
                Console.WriteLine("Error on " + page);
            }
            return new PlayerCollection();
        }

        //Generic web puller for song leaderboards via Hash and Difficulty
        public LeaderboardInfo GetLeaderboardInfo(String hash, String difficulty)
        {
            try
            {
                //https://scoresaber.com/api/leaderboard/by-hash/E42BCDF50EA1F961CB8CEFE502E82806866F6479/info?difficulty=9&gameMode=SoloStandard
                String songInfo = client.DownloadString("https://scoresaber.com/api/leaderboard/by-hash/"+hash+"/info?difficulty="+difficulty+"&gameMode=SoloStandard");
                Console.WriteLine("Unknown Song found and downloaded");
                return JsonConvert.DeserializeObject<LeaderboardInfo>(songInfo, serializerSettings);
            }
            catch
            {
                Console.WriteLine("Error finding song Hash: " + hash + " Difficulty: " + difficulty);
            }
            return new LeaderboardInfo();
        }

        //Generic web puller for song leaderboards via Hash and Difficulty
        public LeaderboardInfo GetLeaderboardInfo(String songID)
        {
            try
            {
                //https://scoresaber.com/api/leaderboard/by-id/382164/info
                String songInfo = client.DownloadString("https://scoresaber.com/api/leaderboard/by-id/" + songID + "/info");
                Console.WriteLine("Unknown Song found and downloaded");
                return JsonConvert.DeserializeObject<LeaderboardInfo>(songInfo, serializerSettings);
            }
            catch
            {
                Console.WriteLine("Error finding song with ID: " + songID);
            }
            return new LeaderboardInfo();
        }
    }
}