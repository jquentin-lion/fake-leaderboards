using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LionStudios.Suite.Leaderboards.Fake
{


    public class RewardsDisplayButton : MonoBehaviour
    {
        [SerializeField] private SubRewardDisplay subRewardPrefab;
        [SerializeField] private Image giftBoxImg;
        [SerializeField] private Button revealBtn;
        private RaceConfig _raceConfiguration;
        private RaceManager _raceManager;

        [SerializeField] private GameObject PopUp;
        [SerializeField] private GameObject PopUpParent;

        private List<SubRewardDisplay> _rewards = new List<SubRewardDisplay>();


        public void Init(RaceConfig raceConfig, RaceManager raceMan, int index)
        {
            _raceConfiguration = raceConfig;
            _raceManager = raceMan;
            if (index > 2)
            {
                gameObject.SetActive(false);
            }
            else
            {
                revealBtn.onClick.RemoveAllListeners();
                revealBtn.onClick.AddListener(RewardsPopUpDisplay);
                giftBoxImg.sprite = raceConfig.AllRewards[index].boxSprite;
                AssignRewardProperties(index);
            }
        }
        private void OnDisable()
        {
            HideRewardPopUp();
        }

        private void RewardsPopUpDisplay()
        {
            PopUp.SetActive(!PopUp.activeInHierarchy);
            Invoke(nameof(this.HideRewardPopUp), 1f);
        }

        public void HideRewardPopUp()
        {
            PopUp.SetActive(false);
            CancelInvoke(nameof(this.HideRewardPopUp));
        }

        private void AssignRewardProperties(int index)
        {
            if (_rewards.Count <= 0)
            {
                for (int i = 0; i < _raceConfiguration.AllRewards[index].Rewards.Count; i++)
                {
                    LeaderboardReward selectedReward = _raceConfiguration.AllRewards[index].Rewards[i];
                    SubRewardDisplay reward = Instantiate(subRewardPrefab, PopUpParent.transform);
                    reward.SetProperties(selectedReward.sprite, selectedReward.amount);
                    _rewards.Add(reward);
                }
            }
            else
            {
                for (int i = 0; i < _rewards.Count; i++)
                {
                    LeaderboardReward selectedReward = _raceConfiguration.AllRewards[index].Rewards[i];
                    SubRewardDisplay reward = _rewards[i];
                    reward.SetProperties(selectedReward.sprite, selectedReward.amount);
                }
            }
        }
    }
}