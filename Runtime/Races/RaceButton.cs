using UnityEngine;
using UnityEngine.UI;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public class RaceButton : MonoBehaviour
    {
        public Image rankImg;
        public GameObject notification;
        public Button raceBtn;
        private RaceConfig _raceConfigurations;
        private RaceManager _raceManager;
        private float playerRankStartValue;
        private int playerIndex;


        private void Awake()
        {
            RaceManager.RaceModuleInitialized += InitButton;
        }

        private void OnEnable()
        {
            if (_raceManager != null)
            {
                _raceManager.CalculateRaceProgress();
            }
        }

        private void InitButton()
        {
            Init(RaceManager.Instance.GetCurrentRaceConfig, RaceManager.Instance);
        }

        private void Init(RaceConfig raceConfig, RaceManager RaceMan)
        {
            _raceManager = RaceMan;
            _raceConfigurations = raceConfig;

            raceBtn.onClick.AddListener(CheckOnBtnClick);

            _raceManager.OnRaceJoined += OnRaceJoinedTriggered;
            _raceManager.OnRaceContinue += OnRaceJoinedTriggered;
            _raceManager.OnRaceFinished += OnRaceFinishTriggered;
            _raceManager.OnLeaderboardCalculation += UpdateButtonUI;
            _raceManager.CalculateRaceProgress();
            UnreadNotification.notificationRead += () => { notification.gameObject.SetActive(false); };
            UnreadNotification.unreadNotificationTrigger += () => { notification.gameObject.SetActive(true); };
            if (_raceManager.RaceFinished)
            {
                UnreadNotification.unreadNotificationTrigger?.Invoke();
            }
        }


        private void Update()
        {
            if (_raceManager == null)
                return;

            if (!_raceManager.JoinedRace)
            {
                rankImg.gameObject.SetActive(false);
            }
        }


        private void OnRaceJoinedTriggered()
        {
            Debug.Log("Race Join Triggered from Button");
        }

        private void OnRaceFinishTriggered()
        {
            UnreadNotification.unreadNotificationTrigger?.Invoke();
        }

        private void UpdateButtonUI(LeaderboardCalculatedData calculatedData)
        {
            SetPlayerIndex(calculatedData);
            // Debug.Log("Updating Button UI");
            _raceManager.RankSpriteTextCalculations(playerIndex, out Sprite rankSprite);
            rankImg.sprite = rankSprite;
            rankImg.gameObject.SetActive(true);
        }


        private void SetPlayerIndex(LeaderboardCalculatedData data)
        {
            if (data.GetPlayerIndex() != playerIndex)
            {
                playerIndex = data.GetPlayerIndex();
                playerRankStartValue = data.GetParticipantList()[data.GetPlayerIndex()].ProgressValue;
                Debug.Log("Rank Changed" + playerRankStartValue + "pl index: " + playerIndex);
            }
        }

        private void CheckOnBtnClick()
        {
            if (_raceConfigurations == null || _raceManager == null)
            {
                Debug.LogError("Race not configured");
                gameObject.SetActive(false);
                return;
            }

            _raceManager.OpenRace();
        }
    }
}