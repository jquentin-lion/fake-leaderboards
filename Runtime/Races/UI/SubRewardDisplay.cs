using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public class SubRewardDisplay : MonoBehaviour
    {
        [SerializeField] internal Image rewardImg;
        [SerializeField] private TextMeshProUGUI amountTxt;

        public void SetProperties(Sprite reward, int amount)
        {
            rewardImg.sprite = reward;
            amountTxt.text = amount.ToString(CultureInfo.InvariantCulture)
                ;
        }
    }
}