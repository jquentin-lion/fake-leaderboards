using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using LionStudios.Suite.Core;
using UnityEngine;

namespace LionStudios.Suite.Leaderboards.Fake
{
    [Serializable]
    internal class RaceUIHolder
    {
        public RaceStartPanel StartPanel;
        public MainRacePanel MainPanel;
        public RaceEndPanel EndPanel;
        public RaceContinuePanel ContinuePanel;
        public Sprite[] InGameRankSprites;
        public Sprite noRewardSprite;
    }

    public class RaceManager : MonoBehaviour, ILeaderboardSystem
    {
        public static RaceManager Instance;
        public Participant participantPrefab;


        [SerializeField] private RaceConfig currentRaceConfig;

        public RaceConfig GetCurrentRaceConfig => currentRaceConfig;

        [SerializeField] internal RaceUIHolder RaceUI;


        internal List<Participant> allParticipants = new List<Participant>();

        internal Action<LeaderboardCalculatedData> OnPlayerProgressed;
        internal Action<LeaderboardCalculatedData> OnLeaderboardCalculation;

        internal Action OnRaceClose;

        internal Action OnRewardsClaimed;

        private const string RaceJoinStatus = "RaceJoinStatus";
        private const string RaceOfferRefuse = "RaceOfferRefuse";
        private const string RaceStartTimePref = "StartTime";
        private const string AutoShowTime = "AutoShowTime";
        private const string RaceEndTimePref = "EndTime";
        private const string FinishCounter = "FinishCounter";
        public const string RACE_SESSION_START_SCORE_KEY = "LS_RaceSessionStartScore";
        public const string RACE_SESSION_START_RANK_KEY = "LS_RaceSessionStartRank";


        public static bool sessionEventFired = false;

        public int PlayerScore
        {
            get
            {
                if (currentRaceConfig.Leaderboard.scoresStorage.GetStoredPlayerScore(StartTime) == null)
                {
                    return 0;
                }

                return currentRaceConfig.Leaderboard.scoresStorage.GetStoredPlayerScore(StartTime).score;
            }
        }

        public bool JoinedRace
        {
            get => bool.Parse(LionStorage.GetString(RaceJoinStatus, "false"));
            internal set
            {
                if (value)
                {
                    OnRaceJoined?.Invoke();
                }

                LionStorage.SetString(RaceJoinStatus, value.ToString(CultureInfo.InvariantCulture));
            }
        }

        internal bool DenyOffer
        {
            get => bool.Parse(LionStorage.GetString(RaceOfferRefuse, "false"));
            set
            {
                if (value)
                {
                    OnRaceJoinDeclined?.Invoke();
                }

                LionStorage.SetString(RaceOfferRefuse, value.ToString(CultureInfo.InvariantCulture));
            }
        }

        public DateTime StartTime
        {
            get => DateTime.Parse(LionStorage.GetString(RaceStartTimePref), CultureInfo.InvariantCulture);
            set => LionStorage.SetString(RaceStartTimePref, value.ToString(CultureInfo.InvariantCulture));
        }

        public DateTime LastPopUpShowTime
        {
            get => DateTime.Parse(LionStorage.GetString(AutoShowTime, DateTime.Now.ToString(CultureInfo.InvariantCulture)), CultureInfo.InvariantCulture);
            set => LionStorage.SetString(AutoShowTime, value.ToString(CultureInfo.InvariantCulture));
        }

        public DateTime EndTime
        {
            get => DateTime.Parse(LionStorage.GetString(RaceEndTimePref), CultureInfo.InvariantCulture);
            set => LionStorage.SetString(RaceEndTimePref, value.ToString(CultureInfo.InvariantCulture));
        }

        public bool RaceFinished { get; set; }

        public int RaceFinishCount
        {
            get => LionStorage.GetInt(FinishCounter);
            set => LionStorage.SetInt(FinishCounter, value);
        }

        public int SessionStartScore
        {
            get => LionStorage.GetInt(RACE_SESSION_START_SCORE_KEY, 0);
            set => LionStorage.SetInt(RACE_SESSION_START_SCORE_KEY, value);
        }

        public int SessionStartRank
        {
            get => LionStorage.GetInt(RACE_SESSION_START_RANK_KEY, 0);
            set => LionStorage.SetInt(RACE_SESSION_START_RANK_KEY, value);
        }

        internal bool raceContinueEligible => currentRaceConfig.AdvancedProperties.ReofferRaceAfterSessionExit && !RaceFinished && JoinedRace;

        public Action OnRaceJoined;
        public event Action OnRaceContinue;

        public event Action OnPlayButtonClicked
        {
            add => MainRacePanel.onPlayButtonClicked += value;
            remove => MainRacePanel.onPlayButtonClicked -= value;
        }

        public Action OnRaceFinished;
        public Action OnRaceJoinDeclined;

        public Action<bool> OnMainRacePanelStateChanged;
        internal static Action RaceModuleInitialized;
        public bool isRaceModuleInitialized;


        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }

            OnLeaderboardCalculation += CheckRaceFinish;
            OnLeaderboardCalculation += UpdateMainUI;
            OnRaceJoinDeclined += () => { currentRaceConfig.Leaderboard.scoresStorage.ClearPastScores(StartTime); };

            if (JoinedRace)
            {
                if (!sessionEventFired)
                {
                    RacesAnalytics.FireSessionEvent(Instance, PlayerScore);
                }
            }
        }


        internal void SetRaceTimes()
        {
            // Debug.Log("Set Start End Time");
            StartTime = DateTime.Now;
            Debug.Log("StartTime:" + StartTime);
            EndTime = StartTime.AddSeconds(currentRaceConfig.Duration);
            Debug.Log("EndTime:" + EndTime);
        }

        internal void ContinueRace()
        {
            OnRaceContinue?.Invoke();
        }

        internal void ContinueRaceTimes(double remainingSeconds)
        {
            EndTime = DateTime.Now.AddSeconds(remainingSeconds);
            Debug.Log("Adjust End Time On Continue:" + EndTime);
        }

        internal void UpdateMainUI(LeaderboardCalculatedData calculatedData)
        {
            List<ParticipantData> data = calculatedData.GetParticipantList();
            int totalScore = currentRaceConfig.Leaderboard.GetLeaderboardData().totalScore;

            for (int i = 0; i < allParticipants.Count; i++)
            {
                ParticipantData currentParticipant = data[i];
                allParticipants[i].isPlayer = calculatedData.GetPlayerIndex() == i;
                float fillValue = (float)currentParticipant.score / totalScore;

                RankSpriteTextCalculations(i, out Sprite rankSprite);
                allParticipants[i].UpdateMyData(currentParticipant.score, (i + 1), fillValue, rankSprite, data[i].profile.name);
            }
        }

        public void OverrideConfig(RaceConfig raceConfig)
        {
            currentRaceConfig = raceConfig;
        }

        public void Initialize()
        {
            if (!JoinedRace)
            {
                SetRaceData();
            }

            OnRaceJoined += OnRaceJoinedTriggered;
            RaceUI.StartPanel.Init(currentRaceConfig, this);
            LeaderboardCalculatedData calculatedData = currentRaceConfig.Leaderboard.CalculatedData(StartTime, EndTime, PlayerScore);
            RaceUI.MainPanel.Init(currentRaceConfig, this, participantPrefab, calculatedData);
            RaceUI.EndPanel.Init(currentRaceConfig, this);
            RaceUI.ContinuePanel.Init(currentRaceConfig, this, participantPrefab, calculatedData);
            AutoDisplayRacePopUp();
            RaceModuleInitialized?.Invoke();
            isRaceModuleInitialized = true;
        }

        internal void SetRaceData()
        {
            SetRaceTimes();
        }

        private void OnRaceJoinedTriggered()
        {
            SetRaceData();
            StartCoroutine(AutoCheckRaceFinish());
            currentRaceConfig.Leaderboard.scoresStorage.
                IncrementPlayerScore(0, currentRaceConfig.Leaderboard.GetLeaderboardData().playerProfile, StartTime, EndTime);
        }

        private void AutoDisplayRacePopUp()
        {
            CalculateRaceProgress();

            if (currentRaceConfig.AdvancedProperties.autoOfferRaceAtStart && !JoinedRace && RaceFinishCount == 0)
            {
                DisplayRaceStartPanel();
            }
            else if (raceContinueEligible)
            {
                DisplayRaceContinuePanel();
            }

            if (DenyOffer && !JoinedRace)
            {
                DateTime nextInviteTime = LastPopUpShowTime.AddSeconds(currentRaceConfig.MinInviteInterval);
                if (nextInviteTime <= DateTime.Now)
                {
                    DisplayRaceStartPanel();
                }
                else
                {
                    Debug.Log("Min Invite Interval not passed");
                }
            }
        }

        public void OpenRace(Action onClose = null)
        {
            OnRaceClose = onClose;
            UnreadNotification.notificationRead?.Invoke();
            if (!JoinedRace)
            {
                DisplayRaceStartPanel();
            }
            else
            {
                if (RaceFinished)
                {
                    RaceUI.EndPanel.gameObject.SetActive(true);
                }
                else
                {
                    RaceUI.MainPanel.gameObject.SetActive(true);
                    RacesAnalytics.FireCheckEvent(Instance, PlayerScore);
                }
            }
        }

        private void DisplayRaceStartPanel()
        {
            LastPopUpShowTime = DateTime.Now;
            RaceUI.StartPanel.gameObject.SetActive(true);
        }

        private void DisplayRaceContinuePanel()
        {
            LastPopUpShowTime = DateTime.Now;
            RaceUI.ContinuePanel.gameObject.SetActive(true);
        }

        public void IncreasePlayerScore(int amount)
        {
            if (currentRaceConfig == null)
            {
                Debug.LogError("Race has not been configured call SetRaceConfiguration method");
                return;
            }

            currentRaceConfig.Leaderboard.scoresStorage.
                IncrementPlayerScore(amount, currentRaceConfig.Leaderboard.GetLeaderboardData().playerProfile, StartTime, EndTime);
            LeaderboardCalculatedData calculatedData = CalculateRaceProgress();
            OnPlayerProgressed?.Invoke(calculatedData);
        }

        public LeaderboardCalculatedData CalculateRaceProgress()
        {
            var calculatedData = currentRaceConfig.Leaderboard.CalculatedData(StartTime, EndTime, PlayerScore);
            OnLeaderboardCalculation?.Invoke(calculatedData);
            return calculatedData;
        }

        private void CheckRaceFinish(LeaderboardCalculatedData calculatedData)
        {
            List<ParticipantData> participants = calculatedData.GetParticipantList();

            if (RaceIsCompletedByScoreWin(participants, currentRaceConfig.Leaderboard.GetLeaderboardData().totalScore))
            {
                RaceFinished = true;
                OnRaceFinished?.Invoke();

                Debug.Log("Race Finished");
            }

            if (RemainingTime.TotalSeconds <= 0 && !RaceFinished && JoinedRace)
            {
                RaceFinished = true;
                //UnreadNotification.notifications++;
                OnRaceFinished?.Invoke();

                Debug.Log("Race Finished");
            }
        }

        public IEnumerator AutoCheckRaceFinish()
        {
            yield return new WaitForEndOfFrame();
            var calculatedData = currentRaceConfig.Leaderboard.CalculatedData(StartTime, EndTime, PlayerScore);
            CheckRaceFinish(calculatedData);
            yield return new WaitForSeconds(1f);
            if (!RaceFinished && JoinedRace)
            {
                StartCoroutine(AutoCheckRaceFinish());
            }
        }


        #region EndRace Stuff

        private bool RaceIsCompletedByScoreWin(List<ParticipantData> allParticipants, int targetScore)
        {
            foreach (var participant in allParticipants)
            {
                if (participant.score >= targetScore)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Utils

        internal void RankSpriteTextCalculations(int index, out Sprite rankSprite)
        {
            //int currentIndex = index > 2 ? RaceUI.InGameRankSprites.Length - 1 : index;

            int currentIndex = index;

            rankSprite = RaceUI.InGameRankSprites[currentIndex];
        }

        internal void RankSpriteEndTextCalculations(int index, out Sprite rankSprite)
        {
            int currentIndex = index > 2 ? RaceUI.InGameRankSprites.Length - 1 : index;
            rankSprite = RaceUI.InGameRankSprites[index];
        }

        internal string TotalTimeDifferenceString()
        {
            return TimeSpan.FromSeconds(currentRaceConfig.Duration).DynamicFormat();
        }



        #endregion

        public bool HasJoined => JoinedRace;

        public bool HasOutdatedScores()
        {
            return RaceFinished;
        }

        public DateTime NextEndTime => EndTime;

        public TimeSpan RemainingTime => NextEndTime - DateTime.Now;
        public string RemainingTimeString => RemainingTime.DynamicFormat();
    }
}