using System;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace LinkedData
{
    public class Top10kPlayers
    {
        public List<Top10kPlayer> top10kPlayers = new List<Top10kPlayer>();

        public String GetJSON()
        {
            return JsonConvert.SerializeObject(top10kPlayers);
        }

        public void SetJSON(String top10kPlayersJSON)
        {
            JsonSerializerSettings serializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            top10kPlayers = JsonConvert.DeserializeObject<List<Top10kPlayer>>(top10kPlayersJSON, serializerSettings);
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