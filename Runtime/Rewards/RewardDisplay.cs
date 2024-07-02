using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public class RewardDisplay : MonoBehaviour
    {
        [SerializeField] private GameObject rotatingLight;
        [SerializeField] internal Image iconImg;
        [SerializeField] private TMP_Text amountLbl;
        
        public void Init(LeaderboardReward reward, bool showRotatingLightBackground)
        {
            iconImg.sprite = reward.sprite;
            amountLbl.text = reward.amount.ToString();
            if(rotatingLight != null)
            {
                rotatingLight.SetActive(showRotatingLightBackground);
            }
        }

        
    }
}
