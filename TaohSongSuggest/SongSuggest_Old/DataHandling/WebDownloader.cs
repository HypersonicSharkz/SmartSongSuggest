using System;
using System.Net;
using ScoreSabersJson;
using Newtonsoft.Json;
using DataHandling;
using System.IO;
using Data;

namespace WebDownloading
{
    public class WebDownloader
    {
        public ToolBox toolBox { get; set; }

        public WebClient client = new WebClient();
        public JsonSerializerSettings serializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };


        public WebDownloader()
        {
            //Adding Tls12 to allowed protocols to be able to download from the GIT.
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
        }

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

        public void UpdateLinkData()
        {
            toolBox.status = "Getting Files.meta";
            //Let us check meta for updates.
            string metaWebPath = "https://raw.githubusercontent.com/HypersonicSharkz/SmartSongSuggest/master/TaohSongSuggest/Configuration/InitialData/Files.meta";
            string metaText = client.DownloadString(metaWebPath);

            FilesMeta filesMetaWeb = JsonConvert.DeserializeObject<FilesMeta>(metaText, serializerSettings);
            FilesMeta filesMetaLocal = toolBox.fileHandler.LoadFilesMeta();

            //Check if local version is same as server version, else download the server version.
            //Also pulls an update of song library to add to local library.
            if (filesMetaWeb.top10kUpdated != filesMetaLocal.top10kUpdated)
            {
                //Top 10k Download
                //where to get the files from and where to save it.
                string webPath = "https://raw.githubusercontent.com/HypersonicSharkz/SmartSongSuggest/master/TaohSongSuggest/Configuration/InitialData/Top10KPlayers.json";
                string filePath = toolBox.fileHandler.top10kPlayersPath + "tmp10k.json";
                string currentJSONPath = toolBox.fileHandler.top10kPlayersPath + "Top10KPlayers.json";

                //Download the file to a tmp file, and when done replace current file.
                toolBox.status = "Downloading Player Scores";
                client.DownloadFile(webPath, filePath);
                //It is fine to delete a file that is not there.
                toolBox.status = "Replacing Player Scores";
                File.Delete(currentJSONPath);
                File.Move(filePath, currentJSONPath);

                //Song Library pull
                webPath = "https://raw.githubusercontent.com/HypersonicSharkz/SmartSongSuggest/master/TaohSongSuggest/Configuration/InitialData/SongLibrary.json";
                filePath = toolBox.fileHandler.songLibraryPath + "tmplib.json";
                //Download the file to a tmp file, add contents, and delete tmp when done.
                toolBox.status = "Downloading Song Library";
                client.DownloadFile(webPath, filePath);
                toolBox.status = "Combining Song Libraries";
                toolBox.fileHandler.AddSongLibrary(filePath);
                toolBox.status = "Removing Temporary Library";
                File.Delete(filePath);

                //save related Files.meta
                toolBox.fileHandler.SaveFilesMeta(filesMetaWeb);
            }
            //toolBox.fileHandler.SaveLinkedData(client.DownloadString(webPath));
            
        }
    }
}