using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public class RankRewardsDisplay : MonoBehaviour
    {

        [SerializeField] private RewardDisplay prefab;

        [SerializeField] private RewardBoxDisplay boxPrefab;
        
        [SerializeField] private bool forceUnboxed;

        internal List<Image> chestRewards { get; private set; } = new List<Image>();
        
        public void Init(RankRewards rankRewards, bool animateRotatingLights)
        {
            chestRewards.Clear();
            bool hasRewards = rankRewards != null;
            gameObject.SetActive(hasRewards);
            if (!hasRewards) 
                return;
            transform.DestroyChildrenImmediate();
                
            if (rankRewards.isBoxed && !forceUnboxed) 
            {
                RewardBoxDisplay instance = Instantiate(boxPrefab, transform);
                instance.Init(rankRewards, animateRotatingLights);
                instance.name = "Reward";
            }
            else
            {
                for (var i = 0; i < rankRewards.Rewards.Count; i++)
                {
                    LeaderboardReward reward = rankRewards.Rewards[i];
                    RewardDisplay instance = Instantiate(prefab, transform);
                    instance.Init(reward, true);
                    instance.name = "Reward";
                    chestRewards.Add(instance.iconImg);
                }
            }
        }
    }
}
