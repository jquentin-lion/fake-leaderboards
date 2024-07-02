using System;
using System.Collections.Generic;
using UnityEngine;

namespace LionStudios.Suite.Leaderboards.Fake
{
    [Serializable]
    public class PseudoBetaDistLeaderboardData : LeaderboardData
    {
        
        public TextAsset botNamesFile;
        
        public int numberOfBots = 20;
        
        [Tooltip("A Percentage of (maximum - minimum) value")]
        [Range(0, 50)]
        public int margin = 10;
        
        public override List<IBaseBotData> GetBaseBots()
        {
            return null;
        }

    }
}
