using System;
using System.Collections.Generic;
using UnityEngine;

namespace LionStudios.Suite.Leaderboards.Fake
{
    [RequireComponent(typeof(Animation))]
    [Serializable]
    public class LeaderboardInstanceHolder : MonoBehaviour
    {
        public DynamicScoreLeaderboard dynamicScoreLeaderboard;
        public PseudoBetaDistLeaderboard probabilityDistributionLeaderboard;
        public List<AnimationCurve> playerAnimationCurve;
    }
}
