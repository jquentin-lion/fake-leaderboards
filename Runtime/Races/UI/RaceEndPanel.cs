using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using LionStudios.Suite.Core;
using LionStudios.Suite.UiCommons;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public class RaceEndPanel : MonoBehaviour
    {
        [SerializeField] private Button claimBtn;
        [SerializeField] private TextMeshProUGUI claimTxt;

        [SerializeField] private Image mainReward;
        [SerializeField] private TextMeshProUGUI rewardLabelTxt;


        [SerializeField] private SubRewardDisplay subRewardPrefab;
        [SerializeField] private RectTransform subRewardsParent;

        [SerializeField] private Image rankImg;
        [SerializeField] private TextMeshProUGUI rankTxt;


        [SerializeField] private GameObject losingWindow;
        [SerializeField] private GameObject winningWindow;

        private RaceConfig _raceConfiguration;
        private RaceManager _raceManager;
        private Canvas sourceCanvas;

        [SerializeField] private List<LeaderboardReward> playerReceivedRewards = new List<LeaderboardReward>();
        private List<SubRewardDisplay> allSpawnedRewards = new List<SubRewardDisplay>();

        private void OnEnable()
        {
            UnreadNotification.notificationRead?.Invoke();

            LeaderboardCalculatedData calculatedData =
                _raceConfiguration.Leaderboard.CalculatedData(_raceManager.StartTime, _raceManager.EndTime,
                    _raceManager.PlayerScore);
            CheckForRewards(calculatedData);
            SendCompleteEvent(calculatedData);
        }

        public void Init(RaceConfig raceConfig, RaceManager raceMan)
        {
            _raceConfiguration = raceConfig;
            _raceManager = raceMan;
            sourceCanvas = transform.GetComponent<Canvas>();
            claimBtn.onClick.AddListener(ClaimClick);
        }

        private async void ClaimClick()
        {
            _raceManager.SetRaceData();
            _raceManager.JoinedRace = false;
            _raceManager.RaceFinished = false;
            _raceConfiguration.Leaderboard.scoresStorage.ClearPastScores(_raceManager.StartTime);
            Debug.Log("Player Receive Reward:" + playerReceivedRewards.Count);

            if (playerReceivedRewards.Count==0)
            {
                OnRaceComplete();
            }
            else
            {
                for (var i = 0; i < playerReceivedRewards.Count; i++)
                {
                    await Task.Delay(150);
                    Reward reward = playerReceivedRewards[i];
                    LionGameInterfaces.Transactions.Earn(reward);
                    RewardFlyAnimation.Spawn(
                        allSpawnedRewards[i].rewardImg,
                        reward.amount,
                        allSpawnedRewards[i].transform,
                        sourceCanvas,
                        reward.id,
                        ScreenAnimations);
                }   
            }
            
            _raceManager.OnRaceClose?.Invoke();
            _raceManager.OnRewardsClaimed?.Invoke();

            async void ScreenAnimations()
            {
                transform.GetChild(0).gameObject.SetActive(false);
                await Task.Delay(TimeSpan.FromSeconds(2f));
                OnRaceComplete();
            }
            
            void OnRaceComplete()
            {
                _raceManager.OpenRace();
                gameObject.SetActive(false);
                mainReward.gameObject.SetActive(true);
                subRewardsParent.gameObject.SetActive(false);
                transform.GetChild(0).gameObject.SetActive(true);
            }
        }
        
        private async void CheckForRewards(LeaderboardCalculatedData calculatedData)
        {
            playerReceivedRewards = CalculateRewards(calculatedData);
            int playerIndex = calculatedData.GetPlayerIndex();

            _raceManager.RankSpriteEndTextCalculations(playerIndex, out Sprite endRankSprite);
            if (playerIndex < 3)
            {
                rewardLabelTxt.gameObject.SetActive(true);
                winningWindow.SetActive(true);
                losingWindow.SetActive(false);
                SetTexts($"Congratulations for finishing {playerIndex + 1}{GetOrdinalIndicator(playerIndex + 1)}!",
                    "Claim");
                rankImg.sprite = endRankSprite;
                if (playerReceivedRewards.Count > 0)
                {
                    claimBtn.gameObject.SetActive(false);
                    claimBtn.gameObject.SetActive(true);
                    subRewardsParent.gameObject.SetActive(true);
                    SpawnSubRewards(playerReceivedRewards);
                }
            }
            else
            {
                winningWindow.SetActive(false);
                losingWindow.SetActive(true);
                rewardLabelTxt.gameObject.SetActive(false);
                SetTexts($"You finished {playerIndex + 1}th", "Continue");
                rankImg.sprite = endRankSprite;
                mainReward.sprite = _raceManager.RaceUI.noRewardSprite;
            }
        }

        private void SpawnSubRewards(List<LeaderboardReward> allRewards)
        {
            if (subRewardsParent.childCount > 0)
            {
                subRewardsParent.DestroyChildren();
                allSpawnedRewards.Clear();
            }

            for (int i = 0; i < allRewards.Count; i++)
            {
                SubRewardDisplay reward = Instantiate(subRewardPrefab, subRewardsParent);
                reward.SetProperties(allRewards[i].sprite, allRewards[i].amount);
                allSpawnedRewards.Add(reward);
            }
        }

        public List<LeaderboardReward> GiveRewards(int rankOfPlayer)
        {
            List<LeaderboardReward> rewards = new List<LeaderboardReward>();
            for (int i = 0; i < _raceConfiguration.AllRewards.Count; i++)
            {
                if (rankOfPlayer == i)
                {
                    rewards = _raceConfiguration.AllRewards[i].Rewards;
                }
            }

            return rewards;
        }

        internal List<LeaderboardReward> CalculateRewards(LeaderboardCalculatedData calculatedData)
        {
            return GiveRewards(calculatedData.GetPlayerIndex());
        }

        public void SetTexts(string title, string claimBtnTxt)
        {
            rankTxt.text = title?.ToString(CultureInfo.InvariantCulture);
            claimTxt.text = claimBtnTxt.ToString(CultureInfo.InvariantCulture);
        }

        private void SendCompleteEvent(LeaderboardCalculatedData calculatedData)
        {
            List<ParticipantData> participants = calculatedData.GetParticipantList();
            ParticipantData playerData = participants[calculatedData.GetPlayerIndex()];

            RacesAnalytics.SendRaceCompleteEvent(
                _raceManager.GetCurrentRaceConfig,
                playerData.score,
                calculatedData.GetPlayerIndex() + 1
            );
        }

        private string GetOrdinalIndicator(int playerPlace)
        {
            if (playerPlace == 1)
            {
                return "st";
            }

            if (playerPlace == 2)
            {
                return "nd";
            }

            if (playerPlace == 3)
            {
                return "rd";
            }

            return "th";
        }
    }
}