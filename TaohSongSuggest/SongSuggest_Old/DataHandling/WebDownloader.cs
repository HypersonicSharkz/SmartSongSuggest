using System;
using System.Net;
using ScoreSabersJson;
using Newtonsoft.Json;
using SongSuggestNS;
using System.IO;
using Data;

namespace WebDownloading
{
    public class WebDownloader
    {
        public SongSuggest songSuggest { get; set; }

        private WebClient client = new WebClient();
        private JsonSerializerSettings serializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

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
            songSuggest.status = "Getting Files.meta";
            //Let us check meta for updates.
            string metaWebPath = "https://raw.githubusercontent.com/HypersonicSharkz/SmartSongSuggest/master/TaohSongSuggest/Configuration/InitialData/Files.meta";
            string metaText = client.DownloadString(metaWebPath);

            Console.WriteLine(metaText);

            FilesMeta filesMetaWeb = JsonConvert.DeserializeObject<FilesMeta>(metaText, serializerSettings);
            FilesMeta filesMetaLocal = songSuggest.fileHandler.LoadFilesMeta();

            //Check if local version is same as server version, else download the server version.
            //Also pulls an update of song library to add to local library.
            //Do not like that filepaths are so deep in data. Might need a better way of handling it.
            if (filesMetaWeb.top10kUpdated != filesMetaLocal.top10kUpdated)
            {
                //Top 10k Download
                //where to get the files from and where to save it.
                string webPath = "https://raw.githubusercontent.com/HypersonicSharkz/SmartSongSuggest/master/TaohSongSuggest/Configuration/InitialData/Top10KPlayers.json";
                string filePath = songSuggest.fileHandler.filePathSettings.top10kPlayersPath + "tmp10k.json";
                string currentJSONPath = songSuggest.fileHandler.filePathSettings.top10kPlayersPath + "Top10KPlayers.json";

                //Download the file to a tmp file, and when done replace current file.
                songSuggest.status = "Downloading Player Scores";
                client.DownloadFile(webPath, filePath);
                //It is fine to delete a file that is not there, so no need to check if it is there first.
                songSuggest.status = "Replacing Player Scores";
                File.Delete(currentJSONPath);
                File.Move(filePath, currentJSONPath);

                //Song Library pull
                webPath = "https://raw.githubusercontent.com/HypersonicSharkz/SmartSongSuggest/master/TaohSongSuggest/Configuration/InitialData/SongLibrary.json";
                filePath = songSuggest.fileHandler.filePathSettings.songLibraryPath + "tmplib.json";
                //Download the file to a tmp file, add contents, and delete tmp when done.
                songSuggest.status = "Downloading Song Library";
                client.DownloadFile(webPath, filePath);
                songSuggest.status = "Combining Song Libraries";
                songSuggest.fileHandler.AddSongLibrary(filePath);
                songSuggest.status = "Removing Temporary Library";
                File.Delete(filePath);


                Console.WriteLine("Web: {0} - Files: {1}", filesMetaWeb.GetLargeVersion(), filesMetaLocal.GetLargeVersion());
                //Check if the data require a profile reset on next update of the activeplayer
                if (filesMetaWeb.GetLargeVersion() != filesMetaLocal.GetLargeVersion())
                {
                    songSuggest.fileHandler.TogglePlayerRefresh();
                }

                //save related Files.meta
                songSuggest.fileHandler.SaveFilesMeta(filesMetaWeb);
            }
        }
    }
}