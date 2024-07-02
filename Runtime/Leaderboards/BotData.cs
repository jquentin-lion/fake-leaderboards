using System;

namespace LionStudios.Suite.Leaderboards.Fake
{

    public interface IBaseBotData
    {
        ParticipantProfile GetProfile();
    }
    
    [Serializable]
    public class BotData : IBaseBotData
    {
        public ParticipantProfile profile;

        public ParticipantProfile GetProfile()
        {
            return profile;
        }
    }
}
