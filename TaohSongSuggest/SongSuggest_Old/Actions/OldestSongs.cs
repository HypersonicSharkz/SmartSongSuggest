using PlaylistNS;
using SongSuggestNS;
using Settings;

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
            songSuggest.RefreshActivePlayer();
            //Create empty playlist, and reset output window.
            playlist = new Playlist(settings.playlistSettings) {songSuggest = songSuggest};

            //Add up to 100 oldest song to playlist
            songSuggest.status = "Finding 100 Oldest";
            playlist.AddSongs(songSuggest.activePlayer.GetOldest(100, settings.ignoreAccuracyEqualAbove, settings.ignorePlayedDays));

            //Generate and save a playlist with the selected songs in the playlist.
            songSuggest.status = "Generating Playlist";
            playlist.Generate();
        }
    }
}