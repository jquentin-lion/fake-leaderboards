using System;

namespace LionStudios.Suite.Leaderboards.Fake
{
    [Serializable]
    public class LeagueRemoteConfig
    {
        public bool IsEnabled;
        public int NumberOfBots;
        public int Duration;
        public int PromoteCount;
        public int AutoUpdateInterval;
        public LeagueRemoteData[] Leagues;
    }
}
