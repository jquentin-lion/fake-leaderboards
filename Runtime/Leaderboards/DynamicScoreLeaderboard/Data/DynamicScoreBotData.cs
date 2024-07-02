using System;
using UnityEngine;

namespace LionStudios.Suite.Leaderboards.Fake
{
    [Serializable]
    public class DynamicScoreBotData : BotData
    {
        [Tooltip("Keep curve value/time between 0 and 1")]
        public AnimationCurve animationCurve;

        [Tooltip("Rank starts from 1")]
        public int hardcodeBotRank = -1;
        
        [Range(0, 1)] public float dynamicClosenessToPlayerScore;
    }
}
