using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LionStudios.Suite.Leaderboards.Fake
{
    [Serializable]
    public class DynamicScoreLeaderboard : ILeaderboard<DynamicScoreLeaderboardData>
    {
        private int _totalScore;
        
        public override LeaderboardCalculatedData CalculatedData(DateTime startTime, DateTime endTime, DateTime currentTime, int playerScore)
        {
            _totalScore = leaderboardData.totalScore;

            if (!IsPassedInitialChecks(startTime, endTime)) 
                return null;
            
            LeaderboardCalculatedData leaderboardCalculatedData = new LeaderboardCalculatedData();
            leaderboardCalculatedData.participantDatas = new List<ParticipantData>();

            float timeSpentPercentage = TimeUtils.GetTimeSpentRatio(startTime, endTime, currentTime) * 100f;

            if (timeSpentPercentage < 0 && timeSpentPercentage > 100)
            {
                Debug.LogError("Leaderboard start and end time is wrong");
                return null;
            }

            var playerData = CalculatePlayerScore(playerScore);
            leaderboardCalculatedData.participantDatas.Add(playerData);
            leaderboardCalculatedData.participantDatas.AddRange(CalculateBotScore(timeSpentPercentage, playerData.score));

            if (leaderboardData.participantsOrderType == ParticipantsOrderType.Descending)
            {
                leaderboardCalculatedData.participantDatas =
                    leaderboardCalculatedData.participantDatas.OrderByDescending(data => data.score).ThenByDescending(data => (playerData.score == 0 ? (data != playerData) : (data == playerData))).ToList();
            }
            else
            {
                leaderboardCalculatedData.participantDatas =
                    leaderboardCalculatedData.participantDatas.OrderBy(data => data.score).ThenBy(data => (playerData.score == 0 ? (data != playerData) : (data == playerData))).ToList();
            }

            int playerIndex = leaderboardCalculatedData.participantDatas.IndexOf(playerData);
            
            leaderboardCalculatedData.playerIndex = playerIndex;

            return leaderboardCalculatedData;
        }

        private bool IsPassedInitialChecks(DateTime startTime, DateTime endTime)
        {
            if (startTime > endTime)
            {
                Debug.LogError("Leaderboard start time must be lower than end time");
                return false;
            }

            return true;
        }

        

        private List<ParticipantData> CalculateBotScore(float timeSpentPercentage, int calculatedPlayerScore)
        {
            List<ParticipantData> participantBots = new List<ParticipantData>();
            
            for (int i = 0; i < leaderboardData.bots.Count; i++)
            {
                DynamicScoreBotData botData = leaderboardData.bots[i];
                float botScore = BasicScoreLogic(timeSpentPercentage, botData);

                float playerScoreRatio = (float) calculatedPlayerScore / _totalScore;
                bool isCriticalStateReachedViaPlayerScore = (playerScoreRatio >= 0.9f);

                //An example if onlineTime is 20, then
                //If time is in first 20%, then simply keep every bot score to it's own curve
                if (timeSpentPercentage < leaderboardData.onlineTime)
                {
                    //Do nothing as bot score is already calculated above
                }
                //If more than 20% time is passed or player is near to win, then do dynamic score logic
                else if(timeSpentPercentage >= leaderboardData.onlineTime || isCriticalStateReachedViaPlayerScore)
                {
                    botScore = DynamicScoreLogic(timeSpentPercentage, botData, botScore, calculatedPlayerScore, 
                        isCriticalStateReachedViaPlayerScore);
                }

                //Clamp bot score between min-max
                botScore = Mathf.
                    Clamp(botScore, 0, _totalScore);

                participantBots.Add(new ParticipantData((int)botScore, botData.profile, _totalScore));
                
            }

            return participantBots;
        }

        private float DynamicScoreLogic(float timeSpentPercentage, DynamicScoreBotData botData,
            float botScore, int calculatedPlayerScore, bool isCriticalStateReachedViaPlayerScore)
        {
            if (botScore > calculatedPlayerScore)
            {
                return botScore;
            }
            
            //If hardcoded rank is given to bot, then perform this 
            if (botData.hardcodeBotRank > 0)
            {
                //Lerp bot score to player score using time spent ratio
                //Means when time is near to end bot score will be more close to player score (OfCourse if bot
                //score is less than player score)
                float playerRatio = (float)calculatedPlayerScore / _totalScore;
                float timeRatio = (float)timeSpentPercentage / 100f;

                //final ratio will depend on 80% of player score and 20% on time passed
                float finalRatio = playerRatio * 0.8f + timeRatio * 0.2f;
                finalRatio = Mathf.Clamp(finalRatio, 0, 1);
                
                botScore = Mathf.Lerp(botScore, calculatedPlayerScore, finalRatio);

                //Rest of remaining 10% time or critical state reached via player score
                if (timeSpentPercentage > 95 || isCriticalStateReachedViaPlayerScore)
                {
                    //Add some value in player score, depending on player hardcode ranking
                    botScore = calculatedPlayerScore + ((_totalScore * 0.1f) / botData.hardcodeBotRank);
                }
            }
            //If dynamic closeness is activated on bot, then just perform this 
            else if (botData.dynamicClosenessToPlayerScore > 0)
            {
                //Lerp bot score to player score using given closeness parameter by devs
                botScore = Mathf.Lerp(botScore, calculatedPlayerScore, botData.dynamicClosenessToPlayerScore);
            }

            return botScore;
        }

        private float BasicScoreLogic(float timeSpentPercentage, DynamicScoreBotData botData)
        {
            float score;
            // Keyframe firstKeyFrame = botData.animationCurve.keys[0];
            // Keyframe lastKeyFrame = botData.animationCurve.keys[botData.animationCurve.keys.Length - 1];
            // float evaluationXAxis = Mathf.Lerp(firstKeyFrame.time, lastKeyFrame.time, timeSpentPercentage / 100f);
            float evaluationXAxis = timeSpentPercentage / 100f;

            evaluationXAxis = Mathf.Clamp(evaluationXAxis, 0, 1);

            //Score = 0 - TotalScore
            float scoreRatio = botData.animationCurve.Evaluate(evaluationXAxis);
            score = scoreRatio * _totalScore;
            return score;
        }

        private int GetMaxScore(int initialScore, int targetAdditionalScore)
        {
            return initialScore + targetAdditionalScore;
        }
        
                
        private ParticipantData CalculatePlayerScore(int playerScore)
        {
            int calculatedPlayerScore = Mathf.Clamp(playerScore,0, leaderboardData.totalScore);
            
            ParticipantData participantPlayer = new 
                ParticipantData(score: calculatedPlayerScore, profile: leaderboardData.playerProfile, leaderboardData.totalScore);

            return participantPlayer;
        }
        
    }
}
