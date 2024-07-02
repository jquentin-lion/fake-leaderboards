using System;

namespace LionStudios.Suite.Leaderboards.Fake
{
    [Serializable]
    public class LeagueRemoteData
    {
        public string Name;
        public int MinimumScore;
        public int MaximumScore;
        public float TargetWinRatio;

        public LeagueRemoteRankReward[] RankRewards;
    }
}
