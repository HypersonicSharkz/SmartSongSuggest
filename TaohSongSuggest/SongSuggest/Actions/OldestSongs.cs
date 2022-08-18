using PlaylistNS;
using SongSuggestNS;
using Settings;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Actions
{
    //Creates a playlist with the 100 oldest maps for a player.
    public class OldestSongs
    {
        public SongSuggest songSuggest { get; set; }
        public Playlist playlist;

        public OldestSongs(SongSuggest songSuggest)
        {
            this.songSuggest = songSuggest;
        }

        public void Oldest100ActivePlayer(OldestSongSettings settings)
        {
            //Create empty playlist, and reset output window.
            playlist = new Playlist(settings.playlistSettings) {songSuggest = songSuggest};
           
            //Add up to 100 oldest song to playlist that has not been banned, and is within given parameters.
            songSuggest.status = "Finding 100 Oldest";
            List<String> oldestSongs = songSuggest.activePlayer.scores
                .Where(c => c.Value.accuracy < settings.ignoreAccuracyEqualAbove)                   //Ignore high Acc
                .Where(c => c.Value.timeSet < DateTime.UtcNow.AddDays(-settings.ignorePlayedDays))  //Ignore newest plays
                .OrderBy(c => c.Value.timeSet)                                                      //Sort list by oldest
                .Select(c => c.Value.songID)                                                        //Get the ID of the songs
                .Except(songSuggest.songBanning.GetPermaBannedIDs())                                //Remove PermaBanned songs
                .Take(100)                                                                          //Get up to 100 songs
                .ToList();                                                                          

            //Lets get them into the playlist.
            playlist.AddSongs(oldestSongs);

            //Generate and save a playlist with the selected songs in the playlist.
            songSuggest.status = "Generating Playlist";
            playlist.Generate();
        }
    }
}