using UnityEngine;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public class LeagueEndScreenChestAnimation : MonoBehaviour
    {
        private LeagueEndScreen endScreen;
        
        void Awake()
        {
            endScreen = GetComponentInParent<LeagueEndScreen>();
        }

        internal void OpenChest()
        {
            GetComponentInChildren<RewardBoxDisplay>().OpenChest();
        }
        
    }
}
