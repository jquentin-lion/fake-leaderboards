using System;
using System.Collections.Generic;

namespace LionStudios.Suite.Leaderboards.Fake
{

    public interface IBaseLeaderboardData
    {
        List<IBaseBotData> GetBaseBots();

        ParticipantProfile GetPlayerProfile();
    }
    
    [Serializable]
    public abstract class LeaderboardData : IBaseLeaderboardData
    {
        public string leaderboardId = "leaderboardId";
        public ParticipantProfile playerProfile;
        public ParticipantsOrderType participantsOrderType;

        public abstract List<IBaseBotData> GetBaseBots();

        public ParticipantProfile GetPlayerProfile()
        {
            return playerProfile;
        }
    }
}
