using System;
using System.Globalization;
using LionStudios.Suite.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public class RaceStartPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI titleTxt;
        [SerializeField] private TextMeshProUGUI descriptionTxt;
        [SerializeField] private TextMeshProUGUI timeRemainingTxt;
        [SerializeField] private JoinButton joinBtn;
        [SerializeField] private Button noThanksBtn;

        private RaceConfig _raceConfiguration;
        private RaceManager _raceManager;

        private void OnEnable()
        {
            SetStartPanelDisplayProperties();
            CheckForEligibility();
        }

        private void CheckForEligibility()
        {
            if (_raceConfiguration.AdvancedProperties.JoinCost == RaceOptInType.InGameCurrency)
            {
                try
                {
                    bool canSpend = LionGameInterfaces.Transactions.CanSpend(_raceConfiguration.AdvancedProperties.joinCurrencyCost.id, _raceConfiguration.AdvancedProperties.joinCurrencyCost.amount);

                    if (!canSpend)
                    {
                        joinBtn.GetComponent<Button>().interactable = false;
                    }
                }
                catch(Exception e)
                {
                    Debug.LogWarning("ILionTransactionsInterface not setup! Please ensure it is initialized");
                }
            }

            if (_raceConfiguration.AdvancedProperties.JoinCost == RaceOptInType.Ad)
            {
                try
                {
                    bool canWatchAd = LionGameInterfaces.Ads.IsRewardedReady();

                    if (!canWatchAd)
                    {
                        joinBtn.GetComponent<Button>().interactable = false;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning("ILionAdsInterface not setup! Please ensure it is initialized");
                }
            }
        }

        public void Init(RaceConfig raceConfig, RaceManager raceMan)
        {
            _raceConfiguration = raceConfig;
            _raceManager = raceMan;
            joinBtn.Init(JoinRace, raceConfig.AdvancedProperties.JoinCost, _raceConfiguration, _raceConfiguration.AdvancedProperties.joinCurrencyCost, "Join");
            noThanksBtn.onClick.AddListener(DenyJoinOffer);
            SetStartPanelDisplayProperties();

            _raceManager.OnRaceJoined += () =>
            {
                gameObject.SetActive(false);
            };
        }

        private void SetStartPanelDisplayProperties()
        {
            SetTexts(_raceConfiguration.RaceTitle, _raceConfiguration.RaceDescription, _raceConfiguration.Leaderboard.GetLeaderboardData().totalScore.ToString(CultureInfo.InvariantCulture));
            DisplayRemainingTime();
        }

        public void SetTexts(string title, string description, string target)
        {
            titleTxt.text = title?.ToString(CultureInfo.InvariantCulture);
            descriptionTxt.text = description?.ToString(CultureInfo.InvariantCulture);
        }


        private void DisplayRemainingTime()
        {
            timeRemainingTxt.text = _raceManager.TotalTimeDifferenceString();
        }

        public void JoinRace()
        {
            _raceManager.DenyOffer = false;
            switch (_raceConfiguration.AdvancedProperties.JoinCost)
            {
                case RaceOptInType.Free:
                    _raceManager.JoinedRace = true;
                    break;
                case RaceOptInType.InGameCurrency:
                    bool returnValueState = LionGameInterfaces.Transactions.Spend(_raceConfiguration.AdvancedProperties.joinCurrencyCost.id, _raceConfiguration.AdvancedProperties.joinCurrencyCost.amount);
                    if (returnValueState)
                    {
                        _raceManager.JoinedRace = true;
                    }
                    else
                    {
                        Debug.LogWarning($"Unable to Join Race");
                    }

                    break;
                case RaceOptInType.Ad:
                    LionGameInterfaces.Ads.ShowRewardedAd("JoinRace", OnAdWatchCompleted);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnAdWatchCompleted(bool success)
        {
            if (success)
            {
                _raceManager.JoinedRace = true;
            }
        }

        private void DenyJoinOffer()
        {
            if (_raceManager.JoinedRace)
            {
                _raceManager.JoinedRace = false;
            }

            _raceManager.DenyOffer = true;
            _raceManager.OnRaceClose?.Invoke();
            gameObject.SetActive(false);
        }
    }
}