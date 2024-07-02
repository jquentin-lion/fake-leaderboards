using System;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public interface ILeaderboardSystem
    {
        
        bool HasJoined { get; }

        bool HasOutdatedScores();
        
        DateTime NextEndTime { get; }
        
        TimeSpan RemainingTime { get; }
        
        string RemainingTimeString { get; }

    }
}
