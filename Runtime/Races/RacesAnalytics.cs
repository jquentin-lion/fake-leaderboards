using System.Collections.Generic;
using LionStudios.Suite.Analytics;
using System;

namespace LionStudios.Suite.Leaderboards.Fake
{
    internal static class RacesAnalytics 
    {
        public static void FireSessionEvent(RaceManager instance, int playerScore)
        {
            var additionalData = new Dictionary<string, object>();

            int rank = instance.CalculateRaceProgress().playerIndex;

            DateTime currentTime = DateTime.Now;
            TimeSpan difference = currentTime.Subtract(instance.StartTime);
            additionalData.Add("time_in_tournament", (int)difference.TotalMinutes);
            additionalData.Add("step_type", "session");
            additionalData.Add("velocity", (float)(instance.CalculateRaceProgress().participantDatas[rank].score) / (DateTime.Now - instance.StartTime).TotalSeconds);

            int score_delta = 0;
            score_delta = instance.CalculateRaceProgress().participantDatas[rank].score - instance.SessionStartScore;
            //Update SessionStartScore last sessions final score
            instance.SessionStartScore = instance.CalculateRaceProgress().participantDatas[rank].score;
            additionalData.Add("score_delta", score_delta);

            int rank_delta = 0;
            rank_delta = ((instance.CalculateRaceProgress().playerIndex) + 1) - instance.SessionStartRank;
            //Update SessionStartScore last sessions final rank
            instance.SessionStartRank = (instance.CalculateRaceProgress().playerIndex + 1);
            additionalData.Add("rank_delta", rank_delta + 1);

            LionAnalytics.MissionStep(false, "race", instance.GetCurrentRaceConfig.RaceTitle, "race", playerScore, null, additionalData);
        }

        public static void FireCheckEvent(RaceManager instance, int playerScore)
        {
            var raceData = instance.CalculateRaceProgress();
            int rank = raceData.playerIndex;
            bool playerIsFirst = (rank == 0);
            int delta_to_1 = 0;
            int delta_to_next = 0;
            int delta_to_previous = 0;

            if (!playerIsFirst)
            {
                delta_to_1 = raceData.participantDatas[0].score - raceData.participantDatas[rank].score;
                delta_to_next = raceData.participantDatas[rank - 1].score - raceData.participantDatas[rank].score;
            }

            if (raceData.participantDatas.Count != (rank + 1))
            {
                delta_to_previous = (raceData.participantDatas[rank + 1].score - raceData.participantDatas[rank].score);
            }

            var additionalData = new Dictionary<string, object>()
            {
                {"delta_to_1", delta_to_1},
                {"delta_to_next", delta_to_next},
                {"delta_to_previous", delta_to_previous},
                {"step_type", "check"}
            };

            LionAnalytics.MissionStep(false, "race", instance.GetCurrentRaceConfig.RaceTitle, "race", playerScore, null, additionalData);
        }
        
        internal static void SendRaceCompleteEvent(RaceConfig config, int playerScore, int playerRank)
        {
            Dictionary<string, object> additionalData = new Dictionary<string, object>()
            {
                { "PlayerRank", playerRank}
            };

            if (playerRank == 1)
            {
                LionAnalytics.MissionCompleted(false,
                    "Race",
                    config.RaceTitle,
                    config.Leaderboard.GetLeaderboardData().leaderboardId,
                    playerScore,
                    null,
                    additionalData
                );
            }
            else
            {
                LionAnalytics.MissionFailed(false,
                    "Race",
                    config.RaceTitle,
                    config.Leaderboard.GetLeaderboardData().leaderboardId,
                    playerScore,
                    null,
                    additionalData
                );
            }
            
        }

    }
}
