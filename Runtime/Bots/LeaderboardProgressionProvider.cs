using System.Collections.Generic;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public abstract class LeaderboardProgressionProvider
    {

        public abstract LeaderboardProgression GetProgression();

    }
    
    
    public class LeaderboardProgression
    {
        internal int playerIndex;
        internal List<ParticipantData> participantDatas;

        public int GetPlayerIndex() => playerIndex;
        public List<ParticipantData> GetParticipantList() => participantDatas;
    }
    
}
