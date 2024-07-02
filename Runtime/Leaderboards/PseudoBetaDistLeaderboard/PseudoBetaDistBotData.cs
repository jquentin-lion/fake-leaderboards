using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LionStudios.Suite.Leaderboards.Fake
{

    [Serializable]
    public class PseudoBetaDistBotData : BotData
    {

        private string playerId;

        public void SetPlayerData(string playerId, ParticipantProfile profile)
        {
            this.profile = profile;
            this.playerId = playerId;
        }
        
        public static int GenerateBotTargetScore(int minimumScore, int maximumScore, float targetWinRatio, int targetValue, int projectedScore, int maxExtendedTrail)
        {
            int botTargetScore;
            if (Random.Range(0f, 1f) <  targetWinRatio)
            {
                int center = (minimumScore + targetValue) / 2;
                if (Random.Range(0f, 1f) < 0.5f)
                    botTargetScore = Mathf.Max(Random.Range(minimumScore, center), Random.Range(minimumScore, center));
                else
                    botTargetScore = Mathf.Min(Random.Range(center, targetValue), Random.Range(center, targetValue));
            }
            else
            {
                // If the projected score is higher than the maximum score, the bot can aim for a score higher than the maximum score
                if (projectedScore > maximumScore)
                {
                    int extendedMax = Mathf.Min(projectedScore, maximumScore + maxExtendedTrail);
                    botTargetScore = Mathf.Min(Random.Range(targetValue, extendedMax), Random.Range(targetValue, extendedMax));
                }
                else
                    botTargetScore = Mathf.Min(Random.Range(targetValue, maximumScore), Random.Range(targetValue, maximumScore));
            }

            return botTargetScore;
        }
        
        public ParticipantData GetParticipantData(TournamentProgress tournamentProgress, int botTargetScore, float normalizedTime, bool store, ref string logs, ParticipantProfile profile)
        {
            if (!tournamentProgress.playerScores.TryGetValue(playerId, out StoredPlayerScore lastScore))
            {
                lastScore = new StoredPlayerScore(tournamentProgress)
                {
                    name = profile.name,
                    score = 0, 
                    normalizedTime = 0f,
                    coefficientValue = Random.Range(0.5f, 2f),
                    targetScore = botTargetScore
                };
            }

            logs += $"ls: {lastScore.score} ; ";
            float progressionRatio = Mathf.Pow(normalizedTime, lastScore.coefficientValue);

            logs += $"cf: {lastScore.coefficientValue} ; ";
            
            //Use this if want to more difference between bots score but the for most bots the score seems to stop updating towards the end of tournament
            int botNewScore = Mathf.Max(lastScore.score,  Mathf.RoundToInt(Mathf.Lerp(0, botTargetScore, progressionRatio)));

            if (store)
            {
                lastScore.score = botNewScore;
                lastScore.normalizedTime = normalizedTime;
                lastScore.targetScore = botTargetScore;
                tournamentProgress.playerScores[playerId] = lastScore;
                tournamentProgress.Save();
            }
            return new ParticipantData(botNewScore, profile, int.MaxValue);
        }

        public int GetLastTargetScore(TournamentProgress tournamentProgress)
        {
            if (tournamentProgress.playerScores.TryGetValue(playerId, out StoredPlayerScore lastScore))
            {
                return lastScore.targetScore;
            }
            else
            {
                return 0;
            }
        }
        
    }
}
