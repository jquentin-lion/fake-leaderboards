using System;
using System.Collections.Generic;
using LionStudios.Suite.Analytics;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public class LeaguesAnalytics
    {

        public enum MissionType
        {
            Completed,
            Failed,
            Abandoned
        }

        public static void FireLeagueJoinedEvent(List<League> leagues, string leagueName, string currentLeague)
        {
            string missionId = $"league_tournament_{currentLeague}";
            LionAnalytics.MissionStarted(false, "league", leagueName, missionId);
        }

        public static void FireLeagueSessionEvent(LeaguesManager instance)
        {
            int rank = instance.GetCurrentScores().playerIndex;
            var additionalData = new Dictionary<string, object>();
            int delta_to_1 = 0;
            int delta_to_next = 0;
            int delta_to_previous = 0;
            int delta_to_promotion = 0;
            bool playerIsFirst = (rank == 0);

            if (!playerIsFirst)
            {
                delta_to_1 = (instance.GetCurrentScores().participantDatas[0].score - instance.GetCurrentScores().participantDatas[rank].score);
                delta_to_next = (instance.GetCurrentScores().participantDatas[rank - 1].score - instance.GetCurrentScores().participantDatas[rank].score);
            }

            if (instance.GetCurrentScores().participantDatas.Count != (rank + 1))
            {
                delta_to_previous = (instance.GetCurrentScores().participantDatas[rank + 1].score - instance.GetCurrentScores().participantDatas[rank].score);
            }

            if (rank != (instance.promoteCount - 1))
            {
                delta_to_promotion = instance.GetCurrentScores().participantDatas[instance.promoteCount - 1].score - instance.GetCurrentScores().participantDatas[rank].score;
            }

            additionalData.Add("step_type", "session");
            additionalData.Add("league_level", instance.leagues[instance.CurrentLeague].name);
            additionalData.Add("zone", LeagueLeaderboardScreen.CheckForZone(instance.CurrentLeague, instance.leagues.Count, rank, instance.promoteCount, instance.GetCurrentScores().participantDatas.Count).ToString());
            additionalData.Add("delta_to_1", delta_to_1);
            additionalData.Add("delta_to_next", delta_to_next);
            additionalData.Add("delta_to_previous", delta_to_previous);
            additionalData.Add("delta_to_promotion", delta_to_promotion);
            additionalData.Add("velocity", (float)(instance.GetCurrentScores().participantDatas[rank].score) / (DateTime.Now - instance.StartTime).TotalSeconds);

            int score_delta = 0;
            score_delta = instance.GetCurrentScores().participantDatas[rank].score - instance.SessionStartScore;
            //Update SessionStartScore last sessions final score
            instance.SessionStartScore = instance.GetCurrentScores().participantDatas[rank].score;
            additionalData.Add("score_delta", score_delta);

            int rank_delta = 0;
            rank_delta = (instance.GetCurrentScores().playerIndex + 1) - instance.SessionStartRank;
            //Update SessionStartScore last sessions final rank
            instance.SessionStartRank = (instance.GetCurrentScores().playerIndex + 1);
            additionalData.Add("rank_delta", rank_delta);

            DateTime currentTime = DateTime.Now;
            TimeSpan difference = currentTime.Subtract(instance.StartTime);
            additionalData.Add("time_in_tournament", (int)difference.TotalMinutes);

            string missionId = $"league_tournament_{instance.CurrentLeague}";

            LionAnalytics.MissionStep(false, "league", instance.leagues[instance.CurrentLeague].name, missionId, instance.GetCurrentScores().participantDatas[rank].score, null, additionalData);
        }

        public static void FireLeagueCheckEvent(List<League> leagues, int currentLeague, LeaderboardCalculatedData scores, int promoteCount)
        {
            int rank = scores.playerIndex;
            int delta_to_1 = 0;
            int delta_to_next = 0;
            int delta_to_previous = 0;
            int delta_to_promotion = 0;
            bool playerIsFirst = (rank == 0);

            var additionalData = new Dictionary<string, object>()
            {
                {"step_type", "check"}
            };

            if (!playerIsFirst)
            {
                delta_to_1 = (scores.participantDatas[0].score - scores.participantDatas[rank].score);
                delta_to_next = (scores.participantDatas[rank - 1].score - scores.participantDatas[rank].score);
            }

            if (scores.participantDatas.Count != (rank + 1))
            {
                delta_to_previous = (scores.participantDatas[rank + 1].score - scores.participantDatas[rank].score);
            }

            if (rank != (promoteCount - 1))
            {
                delta_to_promotion = scores.participantDatas[promoteCount - 1].score - scores.participantDatas[rank].score;
            }

            additionalData.Add("delta_to_1", delta_to_1);
            additionalData.Add("delta_to_next", delta_to_next);
            additionalData.Add("delta_to_previous", delta_to_previous);
            additionalData.Add("delta_to_promotion", delta_to_promotion);
            additionalData.Add("league_level", leagues[currentLeague].name);
            additionalData.Add("zone", LeagueLeaderboardScreen.CheckForZone(currentLeague, leagues.Count, rank, promoteCount, scores.participantDatas.Count).ToString());
            string missionId = $"league_tournament_{currentLeague}";
            LionAnalytics.MissionStep(false, "league", leagues[currentLeague].name, missionId, scores.participantDatas[rank].score, null, additionalData);
        }


        public static void FireLeagueEndEvents(MissionType missionType, LeaguesManager manager, LeaderboardCalculatedData scores)
        {
            int currentTournamentPoints = scores.participantDatas[scores.playerIndex].score;
            int rank = scores.GetPlayerIndex();
            string leagueCounter = $"league_tournament_{manager.CurrentLeague - 1}";

            Dictionary<string, object> additionalData = new Dictionary<string, object>()
            {
                {"league_level", manager.CurrentLeague},
                {"rank", rank + 1},
                {"score", currentTournamentPoints},
            };

            switch (missionType)
            {
                case MissionType.Completed:
                    additionalData.Add("zone", "promotion");
                    LionAnalytics.MissionCompleted(false, "league", manager.leagues[manager.CurrentLeague - 1].name, leagueCounter, currentTournamentPoints, null, additionalData);
                    break;
                case MissionType.Failed:
                    additionalData.Add("zone", "demotion");
                    LionAnalytics.MissionFailed(false, "league", manager.leagues[manager.CurrentLeague + 1].name, leagueCounter, currentTournamentPoints, null, additionalData);
                    break;
                case MissionType.Abandoned:
                    additionalData.Add("zone", "stable");
                    LionAnalytics.MissionAbandoned(false, "league", manager.leagues[manager.CurrentLeague].name, leagueCounter, currentTournamentPoints, null, additionalData);
                    break;
            }
        }

    }
}
