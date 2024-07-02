using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public class JoinButton : MonoBehaviour
    {
        [SerializeField] private Button Btn;
        [SerializeField] private Image iconImg;
        [SerializeField] private Sprite adWatchSprite;
        [SerializeField] private TextMeshProUGUI txtBox;
        [SerializeField] private TextMeshProUGUI continueText;
        [SerializeField] private GameObject HorizontalLayoutGroupTexts;


        private RaceConfig _raceConfig;

        public void Init(UnityAction OnClick, RaceOptInType optInType, RaceConfig raceConfig, Requirement joinRequirement, string JoinTxt = "")
        {
            Btn.onClick.AddListener(OnClick);
            _raceConfig = raceConfig;

            switch (optInType)
            {
                case RaceOptInType.Free:
                    if(continueText != null)
                    {
                        continueText.gameObject.SetActive(true);
                        HorizontalLayoutGroupTexts.SetActive(false);
                    }
                    txtBox.text = JoinTxt;
                    break;
                case RaceOptInType.InGameCurrency:
                    if (continueText != null)
                    {
                        HorizontalLayoutGroupTexts.SetActive(true);
                        continueText.gameObject.SetActive(false);
                    }
                    txtBox.text = joinRequirement.amount.ToString(CultureInfo.InvariantCulture);
                    iconImg.sprite = _raceConfig.AdvancedProperties.joinCurrencyCost.requirement_img;
                    iconImg.gameObject.SetActive(true);
                    break;
                case RaceOptInType.Ad:
                    if (continueText != null)
                    {
                        HorizontalLayoutGroupTexts.SetActive(true);
                        continueText.gameObject.SetActive(false);
                    }
                    txtBox.text = "WATCH";
                    iconImg.sprite = adWatchSprite;
                    iconImg.gameObject.SetActive(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(optInType), optInType, null);
            }
        }
    }
}