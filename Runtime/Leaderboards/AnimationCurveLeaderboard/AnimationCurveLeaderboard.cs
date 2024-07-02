using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LionStudios.Suite.Leaderboards.Fake
{
    /// <summary>
    /// Simple leaderboard in which bots have only animation curve
    /// </summary>
    [Serializable]
    public class AnimationCurveLeaderboard : ILeaderboard<AnimationCurveLeaderboardData>
    {
        
        public override LeaderboardCalculatedData CalculatedData(DateTime startTime, DateTime endTime, DateTime currentTime, int playerScore)
        {
            if (!IsPassedInitialChecks(startTime, endTime))
                return null;
            
            LeaderboardCalculatedData leaderboardCalculatedData = new LeaderboardCalculatedData();
            leaderboardCalculatedData.participantDatas = new List<ParticipantData>();

            float timeSpentRatio = TimeUtils.GetTimeSpentRatio(startTime, endTime, currentTime);

            if (timeSpentRatio < 0 && timeSpentRatio > 1)
            {
                Debug.LogError("Leaderboard start and end time is wrong");
                return null;
            }

            float maxScore = leaderboardData.bots.Max(b => 
                b.progressionCurve.keys[b.progressionCurve.keys.Length - 1].value);

            leaderboardCalculatedData.participantDatas.AddRange(CalculateBotScore(timeSpentRatio, (int) maxScore));
            var playerData = CalculatePlayerScore(playerScore, (int) maxScore);
            leaderboardCalculatedData.participantDatas.Add(playerData);

            if (leaderboardData.participantsOrderType == ParticipantsOrderType.Descending)
            {
                leaderboardCalculatedData.participantDatas =
                    leaderboardCalculatedData.participantDatas.OrderByDescending(data => data.score)
                    .ThenByDescending(data => (playerData.score == 0 ? (data != playerData) : (data == playerData)))
                    .ThenBy(data => data.GetHashCode()).ToList();
            }
            else
            {
                leaderboardCalculatedData.participantDatas =
                    leaderboardCalculatedData.participantDatas.OrderBy(data => data.score)
                    .ThenBy(data => (playerData.score == 0 ? (data != playerData) : (data == playerData)))
                    .ThenBy(data => data.GetHashCode()).ToList();
            }

            int playerIndex = leaderboardCalculatedData.participantDatas.IndexOf(playerData);

            leaderboardCalculatedData.playerIndex = playerIndex;

            return leaderboardCalculatedData;
        }

        private bool IsPassedInitialChecks(DateTime startTime, DateTime endTime)
        {
            if (startTime > endTime)
            {
                Debug.Log("Leaderboard start time must be lower than end time");
                return false;
            }

            return true;
        }

        private ParticipantData CalculatePlayerScore(int playerScore, int maxScore)
        {
            ParticipantData participantPlayer = 
                new ParticipantData(score: playerScore, profile: leaderboardData.playerProfile, maxScore);

            return participantPlayer;
        }

        private List<ParticipantData> CalculateBotScore(float timeSpentRatio, int maxScore)
        {
            List<ParticipantData> participantBots = new List<ParticipantData>();
            
            for (int i = 0; i < leaderboardData.bots.Count; i++)
            {
                AnimationCurveBotData animationCurveBotData = leaderboardData.bots[i];
                Keyframe firstKeyFrame = animationCurveBotData.progressionCurve.keys[0];
                Keyframe lastKeyFrame = animationCurveBotData.progressionCurve.keys[animationCurveBotData.progressionCurve.keys.Length - 1];

                float evaluationXAxis = Mathf.Lerp(firstKeyFrame.time, lastKeyFrame.time, timeSpentRatio);
                int score = Mathf.RoundToInt(animationCurveBotData.progressionCurve.Evaluate(evaluationXAxis) * leaderboardData.globalMultiplier);

                participantBots.Add(new ParticipantData(score, animationCurveBotData.profile, maxScore));
            }

            return participantBots;
        }
    }
}
