using System;
using System.Collections.Generic;
using System.Globalization;
using LionStudios.Suite.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public class RaceContinuePanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timeRemainingTxt;
        [SerializeField] private RewardsDisplayButton[] rewardsPlaceHolders;
        [SerializeField] private JoinButton continueBtn;
        [SerializeField] private Button noThanksBtn;

        [SerializeField] private TextMeshProUGUI descriptionTxt;


        private RaceConfig _raceConfiguration;
        private RaceManager _raceManager;


        public RectTransform parentTransformForPlayers;

        private DateTime tempTimeAtContinue;

        private void OnEnable()
        {
            tempTimeAtContinue = DateTime.Now;
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
                        continueBtn.GetComponent<Button>().interactable = false;
                    }
                }
                catch (Exception e)
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
                        continueBtn.GetComponent<Button>().interactable = false;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning("ILionAdsInterface not setup! Please ensure it is initialized");
                }
            }
        }

        private void OnDisable()
        {
            for (int i = 0; i < rewardsPlaceHolders.Length; i++)
            {
                rewardsPlaceHolders[i].HideRewardPopUp();
            }
        }

        public void Init(RaceConfig raceConfig, RaceManager raceMan, Participant participantPrefab, LeaderboardCalculatedData calculatedData)
        {
            _raceConfiguration = raceConfig;
            _raceManager = raceMan;
            continueBtn.Init(ContinueRace, _raceConfiguration.AdvancedProperties.ReturnJoinCost, _raceConfiguration, _raceConfiguration.AdvancedProperties.returnJoinCurrencyCost, "Continue");
            SetTexts(_raceConfiguration.RaceDescription);
            noThanksBtn.onClick.AddListener(DenyJoinOffer);
            SetRewards();
            DisplayRemainingTime();

            _raceManager.OnRaceContinue += () =>
            {
                double remainingSeconds = _raceManager.EndTime.Subtract(tempTimeAtContinue).TotalSeconds;
                _raceManager.ContinueRaceTimes(remainingSeconds);
                _raceManager.RaceUI.ContinuePanel.gameObject.SetActive(false);
                _raceManager.RaceUI.MainPanel.gameObject.SetActive(true);
            };

            SpawnAllPlayers(participantPrefab, calculatedData);

        }


        public void SpawnAllPlayers(Participant participantPrefab, LeaderboardCalculatedData leaderboardData)
        {
            List<ParticipantData> allParticipant = leaderboardData.GetParticipantList();

            for (var index = 0; index < allParticipant.Count; index++)
            {
                Participant participantData = Instantiate(participantPrefab, parentTransformForPlayers);
                participantData.isPlayer = (index == leaderboardData.GetPlayerIndex());
                participantData.rewardsDisplayButton.Init(_raceConfiguration, _raceManager, index);
                _raceManager.RankSpriteTextCalculations(index, out Sprite rankSprite);
                participantData.UpdateMyData(allParticipant[index].score, index == leaderboardData.GetPlayerIndex() ? allParticipant.Count : (index + 1), allParticipant[index].ProgressValue, rankSprite, allParticipant[index].profile.name);
            }
        }


        private void SetRewards()
        {
            for (int i = 0; i < rewardsPlaceHolders.Length; i++)
            {
                rewardsPlaceHolders[i].Init(_raceConfiguration, _raceManager, i);
            }
        }
        public void SetTexts(string description)
        {
            descriptionTxt.text = description?.ToString(CultureInfo.InvariantCulture);
        }

        private void DisplayRemainingTime()
        {
            timeRemainingTxt.text = _raceManager.RemainingTimeString;
        }

        public void ContinueRace()
        {
            switch (_raceConfiguration.AdvancedProperties.ReturnJoinCost)
            {
                case RaceOptInType.Free:
                    _raceManager.ContinueRace();
                    break;
                case RaceOptInType.InGameCurrency:
                    bool returnValueState = LionGameInterfaces.Transactions.Spend(_raceConfiguration.AdvancedProperties.returnJoinCurrencyCost.id, _raceConfiguration.AdvancedProperties.returnJoinCurrencyCost.amount);
                    if (returnValueState)
                    {
                        _raceManager.ContinueRace();
                    }
                    else
                    {
                        Debug.LogWarning($"Unable to Join Race");
                    }

                    break;
                case RaceOptInType.Ad:
                    LionGameInterfaces.Ads.ShowRewardedAd("ContinueRace", OnAdWatchCompleted);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnAdWatchCompleted(bool success)
        {
            if (success)
            {
                _raceManager.ContinueRace();
            }
        }

        private void DenyJoinOffer()
        {
            _raceManager.JoinedRace = false;
            _raceManager.DenyOffer = true;
            gameObject.SetActive(false);
        }
    }
}