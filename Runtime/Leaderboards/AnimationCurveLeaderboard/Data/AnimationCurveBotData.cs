using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace LionStudios.Suite.Leaderboards.Fake
{
    [Serializable]
    public class AnimationCurveBotData : BotData
    {
        
        [FormerlySerializedAs("animationCurve")] [Tooltip("Keep curve value/time between 0 and 1")]
        public AnimationCurve progressionCurve;
    }
}
