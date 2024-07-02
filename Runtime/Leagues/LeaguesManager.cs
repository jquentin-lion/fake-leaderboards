using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using LionStudios.Suite.Analytics;
using LionStudios.Suite.Analytics.Events;
using UnityEngine;
using LionStudios.Suite.Core;

namespace LionStudios.Suite.Leaderboards.Fake
{

    [Serializable]
    public class League
    {

        public string name;
        public Sprite icon;
        public Sprite altIcon;
        public Material nameMaterial;
        public Color color = Color.white;
        public RankRewards promotionRewards;
        public List<RankRewards> rewards;
        public int minBotScore = 10;
        public int maxBotScore = 100;
        [Range(0f, 1f)]
        public float targetWinRatio = 0.9f;

        public RankRewards GetRankRewardsCopy(int rank)
        {
            if (rank < rewards.Count)
                return rewards[rank].Copy();
            else
                return null;
        }

        public RankRewards GetPromotionRewardsCopy()
        {
            return promotionRewards.Copy();
        }

    }
    
    public class LeaguesManager : MonoBehaviour, ILeaderboardSystem
    {
        private const string HAS_JOINED_KEY = "LS_HasJoinedLeague";

        private const string CURRENT_LEAGUE_KEY = "LS_CurrentLeagueIndex";

        public const string SESSION_START_SCORE_KEY = "LS_SessionStartScore";

        public const string SESSION_START_RANK_KEY = "LS_SessionStartRank";
        
        private const string LEAGUE_REMOTE_CONFIG_KEY = "LS_LeaguesConfig";
        
        private const float MINIMUM_AUTO_UPDATE_INTERVAL_VALUE = 1f;

        public string id => Leaderboard. GetLeaderboardData().leaderboardId;

        public bool isEnabled = true;
        public bool animatePlayerOnly = false;
        public bool overrideJoin = false;
        [SerializeField]
        public string startTime;

        private bool hasLoggedParseError = false;
        
        private void OnValidate()
        {
            if (!DateTime.TryParse(startTime, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime _))
            {
                Debug.LogError($"Invalid string for startTime {startTime}. Expected format is MM/dd/yyyy HH:mm:ss");
            }
        }

        public DateTime StartTime
        {
            get
            {
                if (DateTime.TryParse(startTime, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    return parsedDate;
                }
                else
                {
                    Debug.LogError($"Invalid string for startTime {startTime}. Expected format is MM/dd/yyyy HH:mm:ss. Using EPOCH time instead.");
                    if (!hasLoggedParseError)
                    {
                        LionAnalytics.ErrorEvent(
                            ErrorEventType.Error, 
                            "Invalid startTime string", 
                            new Dictionary<string, object>(){ { "startTimeString", startTime}});
                        hasLoggedParseError = true;
                    }
                    return DateTime.UnixEpoch;
                }
            }
        }
        
        [TimeSpan("{0:dd}d {0:hh}h {0:mm}m {0:ss}s")]
        public int Duration;

        public DateTime LastStartTime
        {
            get
            {
                DateTime nowTime = DateTime.Now;
                DateTime lastStartTime = nowTime - TimeSpan.FromSeconds((int)((nowTime - StartTime).TotalSeconds) % Duration);

                return lastStartTime.Round();
            }
        }
        
        public DateTime NextEndTime => LastStartTime + TimeSpan.FromSeconds(Duration);

        public int promoteCount = 10;

        public List<League> leagues;
        
        [SerializeField] internal PseudoBetaDistLeaderboard Leaderboard;
        
        [Header("Prefab References (Do not change)")]
        [SerializeField] private LeagueOfferScreen offerScreen;
        [SerializeField] private LeagueLeaderboardScreen leaderboardScreen;
        [SerializeField] private LeagueEndScreen endScreen;
        [SerializeField] private LeagueInfoScreen infoScreen;

        internal bool IsInitialized { private set; get; }

        public Action<LeaguesManager> OnLeagueInitialized;

        internal event Action OnConfigOverridden;

        public static Func<bool,string> CustomInfoCollectionTxt;
        public static bool sessionEventFired = false;

        private static LeaguesManager _instance = null;
        public static LeaguesManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<LeaguesManager>();
                return _instance;
            }
        }

        public bool HasJoined
        {
            get => LionStorage.GetInt($"{HAS_JOINED_KEY}_{id}") > 0;
            private set => LionStorage.SetInt($"{HAS_JOINED_KEY}_{id}", value ? 1 : 0);
        }

        public int CurrentLeague
        {
            get => LionStorage.GetInt($"{CURRENT_LEAGUE_KEY}_{id}", 0);
            private set => LionStorage.SetInt($"{CURRENT_LEAGUE_KEY}_{id}", value);
        }

        public int SessionStartScore
        {
            get => LionStorage.GetInt(SESSION_START_SCORE_KEY, 0);
            set => LionStorage.SetInt(SESSION_START_SCORE_KEY, value);
        }

        public int SessionStartRank
        {
            get => LionStorage.GetInt(SESSION_START_RANK_KEY, GetCurrentScores().participantDatas.Count);
            set => LionStorage.SetInt(SESSION_START_RANK_KEY, value);
        }

        public bool HasOutdatedScores()
        {
            return Leaderboard.scoresStorage.HasOutdatedScores(LastStartTime);
        }

        public LeaderboardCalculatedData GetCurrentScores()
        {
            return Leaderboard.CalculatedData(LastStartTime, NextEndTime, Leaderboard.scoresStorage.GetStoredPlayerScore(LastStartTime)?.score ?? 0);
        }
        
        public LeaderboardCalculatedData GetInitialScores()
        {
            return Leaderboard.GetInitialScores();
        }

        public LeaderboardCalculatedData GetCurrentScoresNoRecalculation()
        {
            //TODO
            DateTime startTime = LastStartTime;
            return Leaderboard.CalculatedData(LastStartTime, NextEndTime, Leaderboard.scoresStorage.GetStoredPlayerScore(LastStartTime)?.score ?? 0);
        }

        public RankRewards GetRankAndPromotionRewards(int rank)
        {
            League league = leagues[CurrentLeague];
            RankRewards result = league.GetRankRewardsCopy(rank);

            bool hasPromotionZone = CurrentLeague < leagues.Count - 1;
            if (hasPromotionZone && rank < promoteCount)
            {
                RankRewards promotionRewards = league.GetPromotionRewardsCopy();

                if(promotionRewards != null)
                {
                    if(result == null)
                    {
                        result = promotionRewards;
                    }
                    else
                    {
                        result.Rewards.AddRange(promotionRewards.Rewards);
                    }
                }
            }

            return result;
        }

        private LeaderboardCalculatedData GetPastScores(TournamentProgress pastScores)
        {
            DateTime startTime = Leaderboard.scoresStorage.GetStartTime(pastScores);
            return Leaderboard.CalculatedData(
                startTime, 
                startTime + new TimeSpan(0,0, Duration), 
                pastScores.playerScores.TryGetValue(ScoresStorage.PLAYER_ID, out var playerScore) ? playerScore.score : 0);
        }

        public LeaderboardCalculatedData GetStoredScores()
        {
            var storedScores = Leaderboard.scoresStorage.GetLastOutdatedScores(LastStartTime);
            return GetPastScores(storedScores);
        }

        private async void Start()
        {
            await TaskWaiter.WaitUntil(() => LionCore.IsInitialized);
            
            var leagueRemoteConfig = LiveOpsController.GetValue<LeagueRemoteConfig>(LEAGUE_REMOTE_CONFIG_KEY, null);

            if (leagueRemoteConfig != null)
            {
                OverrideConfig(leagueRemoteConfig);
            }
            
            UpdateLeaderboardData();
            
            if (HasJoined)
            {
                if (!sessionEventFired)
                {
                    LeaguesAnalytics.FireLeagueSessionEvent(Instance);
                }
            }
            leaderboardScreen.Init(this);
            offerScreen.Init(leagues,overrideJoin, () =>
            {
                HasJoined = true;
                LeaguesAnalytics.FireLeagueJoinedEvent(leagues, leagues[CurrentLeague].name, CurrentLeague.ToString());
                Show();
            });
            OnLeagueInitialized?.Invoke(this);
            IsInitialized = true;
        }


        public void OverrideConfig(LeagueRemoteConfig leagueRemoteConfig)
        {
            this.isEnabled = leagueRemoteConfig.IsEnabled;
            this.Duration = leagueRemoteConfig.Duration;
            this.promoteCount = leagueRemoteConfig.PromoteCount;
            this.Leaderboard.GetLeaderboardData().numberOfBots = leagueRemoteConfig.NumberOfBots;
            for (int i = 0; i < Mathf.Min(this.leagues.Count, leagueRemoteConfig.Leagues.Length); i++)
            {
                LeagueRemoteData leagueRemote = leagueRemoteConfig.Leagues[i];
                var leagueLocal = this.leagues[i];
                leagueLocal.name = leagueRemote.Name;
                leagueLocal.minBotScore = leagueRemote.MinimumScore;
                leagueLocal.maxBotScore = leagueRemote.MaximumScore;
                leagueLocal.targetWinRatio = leagueRemote.TargetWinRatio;

                for (int j = 0; j < Mathf.Min(leagueLocal.rewards.Count, leagueRemote.RankRewards.Length); j++)
                {
                    var rankRewardRemote = leagueRemote.RankRewards[j];
                    var rankRewardLocal = leagueLocal.rewards[j];
                    
                    for (int k = 0; k < Mathf.Min(rankRewardLocal.Rewards.Count, rankRewardRemote.Rewards.Length); k++)
                    {
                        var rewardRemote = rankRewardRemote.Rewards[k];
                        var rewardLocal = rankRewardLocal.Rewards[k];
                        rewardLocal.amount = rewardRemote.Amount;
                        rewardLocal.id = rewardRemote.Id;
                    }
                    if (rankRewardLocal.Rewards.Count > rankRewardRemote.Rewards.Length)
                    {
                        Debug.LogWarning($"Remote number of rewards is lower than local. Removing extra entries.");
                        rankRewardLocal.Rewards.RemoveRange(rankRewardRemote.Rewards.Length, rankRewardLocal.Rewards.Count - rankRewardRemote.Rewards.Length);
                    }
                    else if (rankRewardLocal.Rewards.Count < rankRewardRemote.Rewards.Length)
                    {
                        Debug.LogError($"Remote number of rewards is greater than local. Can't add extra entries, some parameters need to be set locally.");
                    }
                }
                
                if (leagueLocal.rewards.Count > leagueRemote.RankRewards.Length)
                {
                    Debug.LogWarning($"Remote number of rewarded ranks is lower than local. Removing extra entries.");
                    leagueLocal.rewards.RemoveRange(leagueRemote.RankRewards.Length, leagueLocal.rewards.Count - leagueRemote.RankRewards.Length);
                }
                else if (leagueLocal.rewards.Count < leagueRemote.RankRewards.Length)
                {
                    Debug.LogError($"Remote number of rewarded ranks is greater than local. Can't add extra entries, some parameters need to be set locally.");
                }
            }
                
            if (this.leagues.Count > leagueRemoteConfig.Leagues.Length)
            {
                Debug.LogWarning($"Remote number of leagues is lower than local. Removing extra entries.");
                this.leagues.RemoveRange(leagueRemoteConfig.Leagues.Length, this.leagues.Count - leagueRemoteConfig.Leagues.Length);
            }
            else if (this.leagues.Count < leagueRemoteConfig.Leagues.Length)
            {
                Debug.LogError($"Remote number of leagues is greater than local. Can't add extra entries, some parameters need to be set locally.");
            }

            OnConfigOverridden?.Invoke();
        }

        public void Show()
        {
            if (!isEnabled)
            {
                Debug.Log("League is disabled! Not showing");
                return;
            }

            if (!HasJoined)
            {
                offerScreen.Show();
                leaderboardScreen.Hide();
                endScreen.Hide();
                return;
            }

            TournamentProgress storedScores = Leaderboard.scoresStorage.GetLastOutdatedScores(LastStartTime);
            
            if (storedScores != null)
            {
                var scores = GetPastScores(storedScores);
                Leaderboard.scoresStorage.ClearPastScores(LastStartTime);
                endScreen.Init(this, scores, promoteCount);
                endScreen.Show();
                offerScreen.Hide();
                leaderboardScreen.Hide();
            }
            else
            {
                offerScreen.Hide();
                endScreen.Hide();
                leaderboardScreen.Show();

                LeaguesAnalytics.FireLeagueCheckEvent(Instance.leagues, Instance.CurrentLeague, Instance.GetCurrentScores(), promoteCount);
            }
        }

        public void Hide()
        {
            offerScreen.Hide();
            leaderboardScreen.Hide();
            endScreen.Hide();
        }

        internal void ClaimRewards(List<LeaderboardReward> rewards)
        {
            foreach (LeaderboardReward reward in rewards)
            {
                LionGameInterfaces.Transactions.Earn(reward);
            }
        }

        public void ResetLeaderboard()
        {
            leaderboardScreen.InitLeaderboard();
        }

        public void Score(int amount)
        {
            if (!isEnabled)
            {
                Debug.Log("League is disabled! Not increasing score");
                return;
            }

            if (!IsInitialized)
            {
                Debug.LogWarning("League score isn't set, wait for League Manager to initialize first!!");
                return;
            }
            
            Leaderboard.scoresStorage.IncrementPlayerScore(amount, Leaderboard.GetLeaderboardData().playerProfile, LastStartTime, NextEndTime);
        }
        public void ManualJoinLeague()
        {
            offerScreen.Hide();
            HasJoined = true;
            LeaguesAnalytics.FireLeagueJoinedEvent(leagues, leagues[CurrentLeague].name, CurrentLeague.ToString());
            Show();
        }
        internal void LeagueUp()
        {
            CurrentLeague = Mathf.Min(CurrentLeague + 1, leagues.Count);
        }
        
        internal void LeagueDown()
        {
            CurrentLeague = Mathf.Max(CurrentLeague - 1, 0);
        }
        
        internal void UpdateLeaderboardData()
        {
            League current = leagues[CurrentLeague];
            Leaderboard.Init(LastStartTime, current.minBotScore, current.maxBotScore, current.targetWinRatio);
        }
        
        public TimeSpan RemainingTime => NextEndTime - DateTime.Now;
        public string RemainingTimeString => RemainingTime.DynamicFormat();

    }
}
