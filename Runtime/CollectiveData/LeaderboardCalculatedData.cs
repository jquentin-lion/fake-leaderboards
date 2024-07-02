using System.Collections.Generic;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public class LeaderboardCalculatedData
    {
        internal int playerIndex;
        internal List<ParticipantData> participantDatas;

        public int GetPlayerIndex() => playerIndex;
        public List<ParticipantData> GetParticipantList() => participantDatas;

        // internal static LeaderboardCalculatedData FromTournamentProgress(TournamentProgress tournamentProgress, List<IBaseBotData> bots)
        // {
        //     var res = new LeaderboardCalculatedData();
        //     res.participantDatas = new List<ParticipantData>();
        //     foreach (var playerScore in tournamentProgress.playerScores)
        //     {
        //         var profile = bots.Find(b => b.)
        //         var pd = new ParticipantData(playerScore.Value.score, animationCurveBotData.profile, maxScore)
        //         res.participantDatas.Add(new ParticipantData(){profile = new ParticipantProfile(){}});
        //     }
        // }
    }
}