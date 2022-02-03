using PlaylistNS;
using DataHandling;
using Settings;

namespace Actions
{
    //Creates a playlist with the 100 oldest maps for a player.
    class OldestSongs
    {
        public ToolBox toolBox { get; set; }
        public Playlist playlist;

        public OldestSongs(ToolBox toolBox)
        {
            this.toolBox = toolBox;
        }

        public void Oldest100ActivePlayer(OldestSongSettings settings)
        {
            toolBox.RefreshActivePlayer();
            //Create empty playlist, and reset output window.
            playlist = new Playlist(settings.playlistSettings) {toolBox = toolBox};

            //Add up to 100 oldest song to playlist
            toolBox.status = "Finding 100 Oldest";
            playlist.AddSongs(toolBox.activePlayer.GetOldest(100, settings.ignoreAccuracyEqualAbove, settings.ignorePlayedDays));

            //Generate and save a playlist with the selected songs in the playlist.
            toolBox.status = "Generating Playlist";
            playlist.Generate();
            
            toolBox.status = "Ready";
        }
    }
}