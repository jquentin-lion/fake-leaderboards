using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public class LeagueLeaderboardScreen : LeagueScreen
    {
        
        [SerializeField] private LeaguesIconsDisplay leaguesIconsDisplay;
        [SerializeField] private LeaderboardEntriesDisplay entriesDisplay;
        [SerializeField] private Button continueBtn;
        [SerializeField] private TMP_Text continueLbl;
        [SerializeField] public RankRewardsDisplay rewardsPopup;
        [SerializeField] private CurrentLeagueNameDisplay leagueName;
        
        private LeaguesManager leaguesManager;
        private bool isInitializing;

        public enum Zone
        {
            promotion,
            stable,
            demotion
        }

        public void Init(LeaguesManager league)
        {
            leaguesManager = league;
            leaguesIconsDisplay.Init(leaguesManager.leagues, leaguesManager.CurrentLeague);
            leagueName.Init(leaguesManager);
            InitLeaderboard();
        }
        
        private void Awake()
        {
            leaguesManager = GetComponentInParent<LeaguesManager>();
            continueBtn.onClick.AddListener(leaguesManager.Show);
        }
        
        private void OnEnable()
        {
            if (!isInitializing)
            {
                UpdateData(true, true);
                entriesDisplay.FocusOnPlayer();
                leaguesIconsDisplay.Init(leaguesManager.leagues, leaguesManager.CurrentLeague);
            }
        }

        private void Update()
        {
            bool hasOutdatedScores = leaguesManager.HasOutdatedScores();
            if (hasOutdatedScores && !continueBtn.gameObject.activeSelf)
            {
                continueBtn.gameObject.SetActive(true);
                bool hasRewards = leaguesManager.GetRankAndPromotionRewards(leaguesManager.GetStoredScores().playerIndex) != null;
                continueLbl.text = hasRewards ? "CLAIM" : "CONTINUE";
                UpdateData(false, false);
            }
            else if (!hasOutdatedScores && continueBtn.gameObject.activeSelf)
                continueBtn.gameObject.SetActive(false);
        }

        public void UpdateData(bool focusOnPlayer, bool animated)
        {
            bool hasOutdatedScores = leaguesManager.HasOutdatedScores();
            LeaderboardCalculatedData scores = hasOutdatedScores ? leaguesManager.GetStoredScores() : leaguesManager.GetCurrentScores();
            entriesDisplay.UpdateData(scores, focusOnPlayer, animated);
        }

        public void InitLeaderboard()
        {
            isInitializing = true;
            gameObject.SetActive(true);
            bool hasOutdatedScores = leaguesManager.HasOutdatedScores();
            LeaderboardCalculatedData scores = hasOutdatedScores ? leaguesManager.GetStoredScores() : (leaguesManager.HasJoined ? leaguesManager.GetCurrentScores() : leaguesManager.GetInitialScores());
            entriesDisplay.Init(scores, leaguesManager.promoteCount, leaguesManager.CurrentLeague < leaguesManager.leagues.Count - 1, leaguesManager.CurrentLeague > 0, leaguesManager.animatePlayerOnly);
            gameObject.SetActive(false);
            isInitializing = false;
        }

        public static Zone CheckForZone(int CurrentLeague, int leaguesCount, int playerRank, int promoCount, int playerCount)
        {
            bool hasPromotionZone = CurrentLeague < leaguesCount - 1;
            bool hasDemotionZone = CurrentLeague > 0;

            if (hasPromotionZone && playerRank < promoCount)
            {
                return Zone.promotion;
            }

            else if (hasDemotionZone && playerRank >= playerCount - promoCount)
            {
                return Zone.demotion;
            }
            else 
            {
                return Zone.stable;
            }
        }
    }
}
