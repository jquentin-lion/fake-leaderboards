using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public class RaceTrack : MonoBehaviour
    {
        private RaceConfig _raceConfig;
        private RaceManager _raceManager;

        [SerializeField] private GameObject participantPrefab;

        [SerializeField] private RectTransform parentTransform;
        public GameObject notification;
        [SerializeField] private Button raceTrackButton;
        [SerializeField] private List<Participant> allParticipants = new List<Participant>();

        private void Start()
        {
            if (RaceManager.Instance.isRaceModuleInitialized)
            {
                InitRaceTrack();
            }
            else
            {
                RaceManager.RaceModuleInitialized += InitRaceTrack;
            }
        }


        private void InitRaceTrack()
        {
            Init(RaceManager.Instance.GetCurrentRaceConfig, RaceManager.Instance);
        }

        public void Init(RaceConfig config, RaceManager raceMan)
        {
            _raceConfig = config;
            _raceManager = raceMan;

            _raceManager.OnRaceJoined += OnRaceJoinedTriggered;
            _raceManager.OnRaceContinue += OnRaceJoinedTriggered;
            _raceManager.OnRaceFinished += OnRaceFinishTriggered;
            _raceManager.OnRaceJoinDeclined += OnRaceJoinDeclined;
            _raceManager.OnLeaderboardCalculation += UpdateRaceTrack;
            _raceManager.OnRewardsClaimed += ResetTrack;
            raceTrackButton.onClick.AddListener(CheckOnBtnClick);
            LeaderboardCalculatedData calculatedData = _raceManager.CalculateRaceProgress();
            SpawnAllPlayers(calculatedData);
            UnreadNotification.notificationRead += () => { notification.gameObject.SetActive(false); };
            UnreadNotification.unreadNotificationTrigger += () => { notification.gameObject.SetActive(true); };
            if (_raceManager.RaceFinished)
            {
                UnreadNotification.unreadNotificationTrigger?.Invoke();
            }

            if (!_raceManager.JoinedRace)
            {
                gameObject.SetActive(false);
            }
        }


        private void CheckOnBtnClick()
        {
            if (_raceConfig == null || _raceManager == null)
            {
                Debug.LogError("Race not configured coming from race track");
                gameObject.SetActive(false);
                return;
            }

            _raceManager.OpenRace();
        }

        private void OnRaceJoinedTriggered()
        {
            gameObject.SetActive(true);
            Debug.Log("Race Join Triggered from raceTrack");
        }

        private void OnRaceFinishTriggered()
        {
            UnreadNotification.unreadNotificationTrigger?.Invoke();
        }

        private void OnRaceJoinDeclined()
        {
            gameObject.SetActive(false);
        }

        private void UpdateRaceTrack(LeaderboardCalculatedData calculatedData)
        {
            List<ParticipantData> data = calculatedData.GetParticipantList();
            int totalScore = _raceConfig.Leaderboard.GetLeaderboardData().totalScore;

            for (int i = 0; i < allParticipants.Count; i++)
            {
                ParticipantData currentParticipant = data[i];
                allParticipants[i].isPlayer = calculatedData.GetPlayerIndex() == i;
                float fillValue = (float)currentParticipant.score / totalScore;
                _raceManager.RankSpriteTextCalculations(i, out Sprite rankSprite);
                allParticipants[i].UpdateDataMiniRaceTrack((i + 1), fillValue, rankSprite);
            }
        }


        private void SpawnAllPlayers(LeaderboardCalculatedData calculatedData)
        {
            List<ParticipantData> currentParticipants = calculatedData.GetParticipantList();
            for (var index = 0; index < currentParticipants.Count; index++)
            {
                var currentParticipant = currentParticipants[index];
                Participant playerParticipant = Instantiate(participantPrefab, parentTransform).GetComponent<Participant>();
                playerParticipant.GetComponent<RectTransform>().SetSiblingIndex(0);
                playerParticipant.isPlayer = index == calculatedData.GetPlayerIndex();
                _raceManager.RankSpriteTextCalculations(index, out Sprite rankSprite);
                playerParticipant.UpdateDataMiniRaceTrack((index + 1), currentParticipant.ProgressValue, rankSprite);
                allParticipants.Add(playerParticipant);
            }
        }

        private void ResetTrack()
        {
            for (int i = 0; i < allParticipants.Count; i++)
            {
                allParticipants[i].ResetMyDate(i);
                _raceManager.RankSpriteTextCalculations(i, out Sprite rankSprite);
                allParticipants[i].UpdateDataMiniRaceTrack((i + 1), 0f, rankSprite);
            }

            gameObject.SetActive(false);
        }
    }
}