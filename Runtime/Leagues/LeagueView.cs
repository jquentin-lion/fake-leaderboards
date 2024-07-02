using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public class LeagueView : MonoBehaviour
    {
        [SerializeField] private LeaderboardEntriesDisplay entriesDisplay;
        [SerializeField] private CurrentLeagueNameDisplay _currentLeagueNameDisplay;
        [SerializeField] private RemainingTimeDisplay _timeDisplay;
        
        private LeaguesManager leaguesManager;
        private bool isInitializing;

        private const int InitialDelay = 40;

        private async void Awake()
        {
            await Task.Delay(InitialDelay);
            if (LeaguesManager.Instance.IsInitialized)
            {
                Init(LeaguesManager.Instance);
            }
            else
            {
                LeaguesManager.Instance.OnLeagueInitialized += Init;
            }
        }

        private void OnEnable()
        {
            if (!isInitializing && leaguesManager != null)
            {
                SetLeaderboardUpdate();
            }
        }

        private void Update()
        {
            if(leaguesManager == null)
                return;
            
            bool hasOutdatedScores = leaguesManager.HasOutdatedScores();
            if (hasOutdatedScores)
            {
                UpdateData(false, false);
            }
        }

        private void Init(LeaguesManager league)
        {
            leaguesManager = league ?? throw new ArgumentNullException(nameof(league));
            InitLeaderboard();
        }

        private void InitLeaderboard()
        {
            isInitializing = true;
            bool hasOutdatedScores = leaguesManager.HasOutdatedScores();
            LeaderboardCalculatedData scores = hasOutdatedScores
                ? leaguesManager.GetStoredScores()
                : (leaguesManager.HasJoined ? leaguesManager.GetCurrentScores() : leaguesManager.GetInitialScores());
            entriesDisplay.Init(scores, leaguesManager.promoteCount,
                leaguesManager.CurrentLeague < leaguesManager.leagues.Count - 1, leaguesManager.CurrentLeague > 0, leaguesManager.animatePlayerOnly);
            isInitializing = false;

            _currentLeagueNameDisplay.Init(leaguesManager);
            _timeDisplay.Init(leaguesManager);
            SetLeaderboardUpdate();
        }

        private void SetLeaderboardUpdate()
        {
            bool hasOutdatedScores = leaguesManager.HasOutdatedScores();
            if (!hasOutdatedScores)
            {
                UpdateData(true, true);
            }
        }

        private void UpdateData(bool focusOnPlayer, bool animated)
        {
            if (leaguesManager == null) return;

            Debug.Log("Updating Leaderboard Data");
            bool hasOutdatedScores = leaguesManager.HasOutdatedScores();
            LeaderboardCalculatedData scores =
                hasOutdatedScores ? leaguesManager.GetStoredScores() : leaguesManager.GetCurrentScores();
            entriesDisplay.UpdateData(scores, focusOnPlayer, animated);
        }
    }
}