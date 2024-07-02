using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public class Participant : MonoBehaviour
    {
        public bool isPlayer;
        public Image rankImg;
        public TextMeshProUGUI scoreTxt;
        public TextMeshProUGUI nameTxt;
        public GameObject isPlayerIndicator;
        public Slider mySlider;

        public Sprite[] fillSprites;
        public GameObject fillSprite;

        [FormerlySerializedAs("RevealRewardsButton")] public RewardsDisplayButton rewardsDisplayButton;

        public void UpdateMyData(int score, int rank, float fillValue, Sprite iconSprite, string name)
        {
            isPlayerIndicator.SetActive(isPlayer);
            nameTxt.gameObject.SetActive(!isPlayer);
            rankImg.sprite = iconSprite;
            mySlider.value = fillValue;
            scoreTxt.text = score.ToString(CultureInfo.InvariantCulture);
            if (isPlayer)
            {
                fillSprite.GetComponent<Image>().sprite = fillSprites[1];
            }
            else
            {
                fillSprite.GetComponent<Image>().sprite = fillSprites[0];
                nameTxt.text = name;
            }
        }

        public void UpdateDataMiniRaceTrack(int rank, float fillValue, Sprite iconSprite)
        {
            isPlayerIndicator.SetActive(isPlayer);
            rankImg.sprite = iconSprite;
            mySlider.value = fillValue;
        }

        public void ResetMyDate(int rank)
        {
            isPlayer = false;
            mySlider.value = 0f;
        }
    }
}