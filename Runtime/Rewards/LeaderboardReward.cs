using System;
using LionStudios.Suite.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace LionStudios.Suite.Leaderboards.Fake
{
    
    [Serializable]
    public class LeaderboardReward : Reward
    {
        
        [FormerlySerializedAs("reward_img")]
        public Sprite sprite;
        
    }
}
