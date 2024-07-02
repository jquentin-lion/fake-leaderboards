using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace LionStudios.Suite.Leaderboards.Fake
{

    public interface IBaseLeaderboard
    {
        LeaderboardCalculatedData CalculatedData(DateTime startTime, DateTime endTime, int playerScore);
        
        LeaderboardCalculatedData CalculatedData(DateTime startTime, DateTime endTime, DateTime currentTime, int playerScore);

        IBaseLeaderboardData GetBaseLeaderboardData();

    }
    
    [Serializable]
    public abstract class ILeaderboard <LDT> : IBaseLeaderboard
        where LDT : LeaderboardData
    {
        
        [FormerlySerializedAs("data")] [SerializeField] protected LDT leaderboardData;

        private ScoresStorage _scoresStorage;
        public ScoresStorage scoresStorage
        {
            get
            {
                if (_scoresStorage == null)
                    _scoresStorage = new ScoresStorage(leaderboardData.leaderboardId);
                return _scoresStorage;
            }
        }

        public LeaderboardCalculatedData CalculatedData(DateTime startTime, DateTime endTime, int playerScore)
        {
            return CalculatedData(startTime, endTime, DateTime.Now, playerScore);
        }
        
        public abstract LeaderboardCalculatedData CalculatedData(DateTime startTime, DateTime endTime, DateTime currentTime, int playerScore);
        
        public IBaseLeaderboardData GetBaseLeaderboardData()
        {
            return leaderboardData;
        }

        public LDT GetLeaderboardData()
        {
            return leaderboardData;
        }

        protected static int GetProjectedScore(int currentScore, float normalizedTime)
        {
            return Mathf.RoundToInt(currentScore / normalizedTime);
        }

        public virtual void DeleteData()
        {
            
        }

    }
}
