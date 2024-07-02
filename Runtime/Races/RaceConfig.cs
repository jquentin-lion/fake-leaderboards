using System;
using System.Collections.Generic;
using LionStudios.Suite.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace LionStudios.Suite.Leaderboards.Fake
{
    [Serializable]
    public class RaceConfig
    {
        public string RaceTitle;
        public string RaceDescription;
        
        public List<RankRewards> AllRewards = new List<RankRewards>();

        [TimeSpan("{0:dd}d {0:hh}h {0:mm}m {0:ss}s")]
        public int Duration;

        [TimeSpan("{0:dd}d {0:hh}h {0:mm}m {0:ss}s")]
        public int MinInviteInterval;

        public RaceInitProperties AdvancedProperties;
        
        public DynamicScoreLeaderboard Leaderboard;
    }

    [Serializable]
    public class RaceInitProperties
    {
        [Tooltip("This will auto-display Race Popup  when Race Module is initialized")]
        public bool autoOfferRaceAtStart;
        
        [FormerlySerializedAs("JoinType")] 
        public RaceOptInType JoinCost;
        
        [FormerlySerializedAs("joinRequirement")] 
        [ShowWhen("JoinCost", RaceOptInType.InGameCurrency)]
        public Requirement joinCurrencyCost;

        [FormerlySerializedAs("ReOfferRaceOnSessionExit")] 
        [Tooltip("This will remove the player from race everytime they exit the game and when they come back they join the race again")]
        public bool ReofferRaceAfterSessionExit;

        [FormerlySerializedAs("ReturnJoinType")] 
        [ShowWhen("ReofferRaceAfterSessionExit")]
        public RaceOptInType ReturnJoinCost;
        
        [FormerlySerializedAs("returnJoinRequirement")] 
        [ShowWhen(new string[] { "ReturnJoinCost", "ReofferRaceAfterSessionExit"}, new object[]{RaceOptInType.InGameCurrency})]
        public Requirement returnJoinCurrencyCost;

    }
    
    public enum RaceOptInType
    {
        Free = 0,
        InGameCurrency = 1,
        Ad = 2
    }
}