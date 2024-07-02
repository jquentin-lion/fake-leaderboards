using System;
using System.Collections.Generic;
using LionStudios.Suite.Core;
using UnityEngine;
using System.Linq;

namespace LionStudios.Suite.Leaderboards.Fake
{
    [Serializable]
    public class RankRewards : ICloneable
    {
        public bool isBoxed;
        [ShowWhen("isBoxed")]
        public Sprite boxSprite;
        [ShowWhen("isBoxed")]
        public Sprite openedBoxSprite;
        public List<LeaderboardReward> Rewards = new List<LeaderboardReward>();

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public RankRewards Copy()
        {
            RankRewards copy = Clone() as RankRewards;
            copy.Rewards = Rewards.ToList();
            return copy;
        }
    }
}
