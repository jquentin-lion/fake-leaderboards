using System;
using System.Collections;
using System.Threading.Tasks;
using LionStudios.Suite.UiCommons;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public class LeagueEndScreen : LeagueScreen
    {
        
        [SerializeField] private TMP_Text recapTitleLbl;
        [SerializeField] private TMP_Text recapMessageLbl;

        [SerializeField] private Button continueBtn;

        [SerializeField] private TMP_Text continueLbl;

        [SerializeField] private RankRewardsDisplay rankRewardsDisplay;

        [SerializeField] private LeagueDisplay leagueDisplay;

        // For animation
        [SerializeField] private Button openBtn;
        [SerializeField] private GameObject endingAnimation;
        [SerializeField] private RankRewardsDisplay openedBoxRankRewardsDisplay;
        [SerializeField] private GameObject openedParentChestObject;
        [SerializeField] private GameObject rankingInfoSection;
        
        private Canvas sourceCanvas;
        private bool ChestOpened;

        public void Init(LeaguesManager manager, LeaderboardCalculatedData scores, int promoteCount)
        {
            sourceCanvas = transform.GetComponent<Canvas>();
            int rank = scores.GetPlayerIndex();
            bool hasPromotionZone = manager.CurrentLeague < manager.leagues.Count - 1;
            bool hasDemotionZone = manager.CurrentLeague > 0;

            League completedLeague = manager.leagues[manager.CurrentLeague];
            leagueDisplay.Init(completedLeague, -1, -1, false);

            RankRewards rewards = manager.GetRankAndPromotionRewards(rank);

            rankRewardsDisplay.Init(rewards, true);
            openedBoxRankRewardsDisplay.Init(rewards, false);

            if (hasPromotionZone && rank < promoteCount)
            {
                manager.LeagueUp();
                recapTitleLbl.text = $"You finished {rank+1}{StatUtils.OrdinalSuffix(rank+1)}! ";
                recapMessageLbl.text = $"Congratulations! You're promoted to the {manager.leagues[manager.CurrentLeague].name} league!";
                LeaguesAnalytics.FireLeagueEndEvents(LeaguesAnalytics.MissionType.Completed, manager, scores);
            }
            else if (hasDemotionZone && rank >= scores.participantDatas.Count - promoteCount)
            {
                manager.LeagueDown();
                recapTitleLbl.text = $"You finished {rank+1}{StatUtils.OrdinalSuffix(rank+1)}! ";
                recapMessageLbl.text = $"Sorry! You're demoted to the {manager.leagues[manager.CurrentLeague].name} league.\" \n Don't let this pull you down!";
                LeaguesAnalytics.FireLeagueEndEvents(LeaguesAnalytics.MissionType.Failed, manager, scores);
            }
            else
            {
                if ((manager.CurrentLeague + 1) == manager.leagues.Count)
                {
                    recapTitleLbl.text = $"You finished {rank + 1}{StatUtils.OrdinalSuffix(rank + 1)}! ";
                    recapMessageLbl.text = $"You stayed in the {manager.leagues[manager.CurrentLeague].name} league! Keep up the work, champion!";
                }
                else
                {
                    recapTitleLbl.text = $"You finished {rank + 1}{StatUtils.OrdinalSuffix(rank + 1)}! ";
                    recapMessageLbl.text = $"You stayed in the {manager.leagues[manager.CurrentLeague].name} league! \n Better luck next time!";
                }

                LeaguesAnalytics.FireLeagueEndEvents(LeaguesAnalytics.MissionType.Abandoned, manager, scores);
            }
            
            manager.UpdateLeaderboardData();

            if (rewards != null && rewards.isBoxed)
            {
                continueBtn.gameObject.SetActive(false);
                openBtn.gameObject.SetActive(true);
            }
            else
            {
                continueLbl.text = rewards == null ? "CONTINUE" : "CLAIM";
            }

            if (rewards != null)
            {
                manager.ClaimRewards(rewards.Rewards);
            }

            continueBtn.onClick.RemoveAllListeners();
            openBtn.onClick.RemoveAllListeners();

            continueBtn.onClick.AddListener(async () =>
            {
                if (!openedParentChestObject.gameObject.activeInHierarchy)
                {
                    manager.ResetLeaderboard();
                    manager.Show();
                }
                else
                {
                    if (ChestOpened)
                    {
                        for (var i = 0; i < rewards.Rewards.Count; i++)
                        {
                            var reward = rewards.Rewards[i];
                            RewardFlyAnimation.Spawn(
                                openedBoxRankRewardsDisplay.chestRewards[i],
                                reward.amount,
                                openedBoxRankRewardsDisplay.chestRewards[i].transform,
                                sourceCanvas,
                                reward.id,
                                ScreenAnimations);
                            await Task.Delay(150);
                        }
                    }
                }

                async void ScreenAnimations()
                {
                    ChangeBgScreenStatus(false);
                    await Task.Delay(TimeSpan.FromSeconds(2f));
                    manager.ResetLeaderboard();
                    manager.Show();
                    Debug.Log("RWD Should Add!");
                    ChestOpened = false;
                    ChangeBgScreenStatus(true);
                }

                void ChangeBgScreenStatus(bool state)
                {
                    transform.GetChild(0).gameObject.SetActive(state);
                    transform.GetChild(1).gameObject.SetActive(state);
                }

                openBtn.gameObject.SetActive(false);
                openedBoxRankRewardsDisplay.gameObject.SetActive(false);
                rankingInfoSection.SetActive(true);
                openedParentChestObject.SetActive(false);
            });

            openBtn.onClick.AddListener(() =>
            {
                if (rewards != null)
                {
                    if (rewards.isBoxed == true)
                    {
                        Animator animator = endingAnimation.GetComponent<Animator>();
                        if (animator != null)
                        {
                            animator.SetBool("OpenChest", true);
                            openBtn.gameObject.SetActive(false);
                            continueLbl.text = "CLAIM";
                            openedBoxRankRewardsDisplay.Init(rewards, false);

                            RankRewards updatedRewardObject = manager.GetRankAndPromotionRewards(rank);
                            var cachedBoxSprite = updatedRewardObject.boxSprite;
                            updatedRewardObject.boxSprite = updatedRewardObject.openedBoxSprite;
                            rankRewardsDisplay.Init(rewards, false);
                            updatedRewardObject.boxSprite = cachedBoxSprite;
                            ChestOpened = true;
                            return;
                        }
                    }
                }
                manager.ResetLeaderboard();
                manager.Show();
            });

        }
    }
}
