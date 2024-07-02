using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LionStudios.Suite.Leaderboards.Fake
{
    /// <summary>
    /// This leaderboard can have
    /// 1. hard-coded bot ranking.
    /// 2. Score dynamically tries to be close to player score.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class DynamicScoreLeaderboardData : LeaderboardData
    {
        public List<DynamicScoreBotData> bots;
        public int totalScore;
        
        [Tooltip("A percentage of time passed since beginning of race for Dynamic Score & Hardcoded rank activation")]
        [Range(0, 89)] public int onlineTime = 20;
        
        public override List<IBaseBotData> GetBaseBots()
        {
            return bots.Cast<IBaseBotData>().ToList();
        }
    }
}
