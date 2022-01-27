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
            toolBox.SetActivePlayer(settings.scoreSaberID);
            //Create empty playlist, and reset output window.
            playlist = new Playlist(toolBox.fileHandler, toolBox.songLibrary);

            //Add up to 100 oldest song to playlist
            playlist.AddSongs(toolBox.activePlayer.GetOldest(100, settings.ignoreAccuracyEqualAbove, settings.ignorePlayedDays));

            //Generate and save a playlist with the selected songs in the playlist.
            toolBox.fileHandler.SavePlaylist(playlist.Generate(PlaylistType.OldestPlays), "100 Oldest");
        }
    }
}