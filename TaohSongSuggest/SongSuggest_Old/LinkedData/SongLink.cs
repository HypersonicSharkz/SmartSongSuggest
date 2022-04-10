using System;
using SongSuggestNS;


namespace LinkedData
{
    public class SongLink
    {
        public String playerID { get; set; }
        public Top10kScore originSongScore { get; set; }
        public Top10kScore targetSongScore { get; set; }
        private int strength;
        private double distance;
        private bool unsetStrength = true;
        private bool unsetDistance = true;

        public float Strength()
        {
            if (unsetStrength)
            {
                strength = Math.Max((20 - Math.Abs(originSongScore.rank - targetSongScore.rank)), 1);
                unsetStrength = false;
            }
            return strength;
        }

        //Calculates the distance
        public double Distance(SongSuggest songSuggest)
        {
            if (unsetDistance)
            {
                //Calcualte distance.
                //Get the 3 song scores (ActivePlayers, SuggestionOrigin, and SuggestionTarget)
                String originID = originSongScore.songID;
                String targetID = targetSongScore.songID;
                double originMaxPP = GetMaxPP(songSuggest, originID);
                double targetMaxPP = GetMaxPP(songSuggest, targetID);
                
                //Split the 3 scores in PP/Acc and Acc
                Split(out double activePlayerAcc, out double activePlayerPPAcc, songSuggest.activePlayer.scores[originID].pp, originMaxPP);
                Split(out double originPlayerAcc, out double originPlayerPPAcc, originSongScore.pp, originMaxPP);
                Split(out double targetPlayerAcc, out double targetPlayerPPAcc, targetSongScore.pp, targetMaxPP);

                //Reset the distance to 1,1 for ActivePlayers score
                
                double distancePPAcc = (targetPlayerPPAcc - originPlayerPPAcc)/activePlayerPPAcc;
                double distanceAcc = (targetPlayerAcc - originPlayerAcc)/activePlayerAcc;

                //Calculate the distance between the SuggestionOrigin and SuggestionTarget via Pythagoros A^2+B^2=C^2
                distance = Math.Sqrt(distancePPAcc*distancePPAcc + distanceAcc*distanceAcc);
                unsetDistance = false;
            }
            return distance;
        }

        //Splits a PP score into an Acc and a PP per Acc component, using the maxPP value known in the Top10kPlayers data
        private void Split(out double acc, out double ppAcc, double pp, double maxPP)
        {
            acc = maxPP / pp;
            ppAcc = pp / acc;
        }

        private double GetMaxPP(SongSuggest songSuggest, String songID)
        {
            return songSuggest.top10kPlayers.top10kSongMeta[songID].maxScore;
        }
    }
}