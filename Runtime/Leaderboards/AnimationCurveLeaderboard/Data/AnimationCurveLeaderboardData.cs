using System;
using System.Collections.Generic;
using System.Linq;

namespace LionStudios.Suite.Leaderboards.Fake
{
    [Serializable]
    public class AnimationCurveLeaderboardData : LeaderboardData
    {
        public List<AnimationCurveBotData> bots;
        public float globalMultiplier = 1f;

        public override List<IBaseBotData> GetBaseBots()
        {
            return bots.Cast<IBaseBotData>().ToList();
        }
    }
}
