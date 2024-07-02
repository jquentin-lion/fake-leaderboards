using System;
using TMPro;
using UnityEngine;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public class RemainingTimeDisplay : MonoBehaviour
    {
        
        const float INCREMENT_ANGLE = -90f;

        [SerializeField]
        private TMP_Text text;

        [SerializeField] private ILeaderboardSystem leaderboardSystem;
        [SerializeField] private GameObject externalLeaderboardSystemGO;
        [SerializeField] NotJoinedBehavior notJoinedBehavior;
        public enum NotJoinedBehavior
        {
            Join,
            Time
        }


        public void Init(LeaguesManager league)
        {
            externalLeaderboardSystemGO = league.gameObject;
            leaderboardSystem = league.GetComponent<ILeaderboardSystem>();
        }

        private void Start()
        {
            if (leaderboardSystem == null)
            {
                leaderboardSystem = GetComponentInParent<ILeaderboardSystem>();
                if (leaderboardSystem == null && externalLeaderboardSystemGO != null && externalLeaderboardSystemGO.GetComponent<ILeaderboardSystem>() != null)
                    leaderboardSystem = externalLeaderboardSystemGO.GetComponent<ILeaderboardSystem>();
                if (leaderboardSystem == null && GetComponentInParent<LeagueShowButton>() != null)
                    leaderboardSystem = GetComponentInParent<LeagueShowButton>().leaguesManager;
                if (leaderboardSystem == null && GetComponentInParent<RaceButton>() != null)
                    // TODO: use RaceButton's raceManager instead
                    leaderboardSystem = RaceManager.Instance;
            }
        }

        private void Update()
        {
            if(leaderboardSystem==null)
                return;
            
            if (!leaderboardSystem.HasJoined && notJoinedBehavior == NotJoinedBehavior.Join)
                text.text = "JOIN";
            else if (leaderboardSystem.HasOutdatedScores())
                text.text = "Finished";
            else
                text.text = $"{(leaderboardSystem.NextEndTime - DateTime.Now).DynamicFormat()}";
        }
    }
}
