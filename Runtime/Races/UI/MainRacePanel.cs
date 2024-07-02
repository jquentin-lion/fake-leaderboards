using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public class MainRacePanel : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI titleTxt;
        [SerializeField] private TextMeshProUGUI descriptionTxt;
        [SerializeField] private Button[] closeBtn;
        [SerializeField] private Button playBtn;

        public RectTransform parentTransformForPlayers;

        private RaceConfig _raceConfiguration;
        private RaceManager _raceManager;

        public static event Action onPlayButtonClicked;

        public void Init(RaceConfig raceConfig, RaceManager raceMan, Participant participantPrefab, LeaderboardCalculatedData calculatedData)
        {
            _raceConfiguration = raceConfig;
            _raceManager = raceMan;
            foreach (var btn in closeBtn)
            {
                btn.onClick.AddListener(() =>
                {
                    _raceManager.OnRaceClose?.Invoke();
                    gameObject.SetActive(false);
                });
            }

            playBtn.onClick.AddListener(() =>
            {
                gameObject.SetActive(false);
                onPlayButtonClicked?.Invoke();
            });

            _raceManager.OnRaceFinished += () =>
            {
                _raceManager.RaceFinishCount += 1;
                if (gameObject.activeInHierarchy)
                {
                    gameObject.SetActive(false);
                    _raceManager.RaceUI.EndPanel.gameObject.SetActive(true);
                }
            };
            _raceManager.OnMainRacePanelStateChanged += (state) =>
            {
                if (state) return;
                foreach (var participant in _raceManager.allParticipants)
                {
                    participant.rewardsDisplayButton.HideRewardPopUp();
                }
            };

            SpawnAllPlayers(participantPrefab, calculatedData);
        }

        private void OnEnable()
        {
            if (_raceConfiguration != null && _raceManager != null)
            {
                SetTexts(_raceConfiguration.RaceTitle, _raceConfiguration.RaceDescription);
                _raceManager.CalculateRaceProgress();
            }

            _raceManager.OnMainRacePanelStateChanged?.Invoke(true);
        }

        private void OnDisable()
        {
            _raceManager.OnMainRacePanelStateChanged?.Invoke(false);
        }

        public void SpawnAllPlayers(Participant participantPrefab, LeaderboardCalculatedData leaderboardData)
        {
            if (_raceManager.allParticipants.Count > 0)
            {
                _raceManager.CalculateRaceProgress();
                return;
            }

            _raceManager.allParticipants = new List<Participant>();
            List<ParticipantData> allParticipant = leaderboardData.GetParticipantList();

            for (var index = 0; index < allParticipant.Count; index++)
            {
                Participant participantData = Instantiate(participantPrefab, parentTransformForPlayers);
                participantData.isPlayer = (index == leaderboardData.GetPlayerIndex());
                participantData.rewardsDisplayButton.Init(_raceConfiguration, _raceManager, index);
                _raceManager.RankSpriteTextCalculations(index, out Sprite rankSprite);
                participantData.UpdateMyData(0, index == leaderboardData.GetPlayerIndex() ? allParticipant.Count : (index + 1), 0f, rankSprite, allParticipant[index].profile.name);
                _raceManager.allParticipants.Add(participantData);
            }
        }

        public void SetTexts(string title, string description)
        {
            titleTxt.text = title?.ToString(CultureInfo.InvariantCulture);
            descriptionTxt.text = description?.ToString(CultureInfo.InvariantCulture);
        }

    }
}