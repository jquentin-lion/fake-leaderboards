using System;
using System.Collections.Generic;
using System.Linq;
using LionStudios.Suite.Core;
using Newtonsoft.Json;
using UnityEngine;

namespace LionStudios.Suite.Leaderboards.Fake
{

    [Serializable]
    public class StoredPlayerScore
    {
        public string name;
        public int score;
        public float normalizedTime;
        public float coefficientValue;
        public int targetScore;

        private TournamentProgress container;
        
        public StoredPlayerScore(TournamentProgress container)
        {
            this.container = container;
        }

        public void Save()
        {
            container.Save();
        }
        
        public override string ToString()
        {
            return $"Stored Score for '{name}': {score} at {normalizedTime}";
        }

        public void ResetContainers(TournamentProgress container)
        {
            this.container = container;
        }
    }

    [Serializable]
    public class TournamentProgress
    {

        public Dictionary<string, StoredPlayerScore> playerScores = new Dictionary<string, StoredPlayerScore>();

        public int lastTargetUpdateValue = int.MinValue;
        
        private TournamentsProgresses container;
        
        public TournamentProgress(TournamentsProgresses container)
        {
            this.container = container;
        }
        
        public StoredPlayerScore GetScore(string playerId)
        {
            if (!playerScores.TryGetValue(playerId, out StoredPlayerScore res))
                return null;
            return res;
        }

        public void Save()
        {
            container.Save();
        }

        public void ResetContainers(TournamentsProgresses container)
        {
            this.container = container;
            foreach (StoredPlayerScore score in playerScores.Values)
            {
                score.ResetContainers(this);
            }
        }

    }

    [Serializable]
    public class TournamentsProgresses
    {
        
        public Dictionary<long, TournamentProgress> tournaments = new Dictionary<long, TournamentProgress>();

        private ScoresStorage container;
        
        public TournamentsProgresses(ScoresStorage container)
        {
            this.container = container;
        }
        
        public TournamentProgress GetTournament(long startTime)
        {
            if (!tournaments.TryGetValue(startTime, out TournamentProgress res))
                return null;
            return res;
        }

        public void Save()
        {
            container.Save();
        }

        public void ResetContainers(ScoresStorage container)
        {
            this.container = container;
            foreach (TournamentProgress tournamentProgress in tournaments.Values)
            {
                tournamentProgress.ResetContainers(this);
            }
        }

        public void RemoveTournament(long startTime)
        {
            if (tournaments.ContainsKey(startTime))
            {
                tournaments.Remove(startTime);
            }
        }
        
    }

    public class ScoresStorage
    {
        private const string KEY_PREFIX = "LS_StoredTournamentPlayerScore";

        public const string PLAYER_ID = "player";

        private string leaderboardId;

        private string Key => $"{KEY_PREFIX}_{leaderboardId}";

        public TournamentsProgresses tournamentsProgresses;

        public ScoresStorage(string leaderboardId)
        {
            this.leaderboardId = leaderboardId;
            if (LionStorage.HasKey(Key))
            {
                string json = LionStorage.GetString(Key);
                tournamentsProgresses = JsonConvert.DeserializeObject<TournamentsProgresses>(json);
                tournamentsProgresses.ResetContainers(this);
            }
            else
            {
                tournamentsProgresses = new TournamentsProgresses(this);
            }
        }
        
        public void Save()
        {
            LionStorage.SetString(Key, JsonConvert.SerializeObject(tournamentsProgresses));
            LionStorage.Save();
        }
        
        public List<StoredPlayerScore> GetStoredScores(string playerId)
        {
            return tournamentsProgresses.tournaments.Select(kvp => kvp.Value.playerScores[playerId]).ToList();
        }

        public List<StoredPlayerScore> GetStoredPlayerScores()
            => GetStoredScores(PLAYER_ID);
        
        public StoredPlayerScore GetStoredScore(string playerId, DateTime startTime)
        {
            return tournamentsProgresses.GetTournament(startTime.ToUnixTime())?.GetScore(playerId);
        }

        public StoredPlayerScore GetStoredPlayerScore(DateTime startTime)
            => GetStoredScore(PLAYER_ID, startTime);

        public void SetScore(string playerId, ParticipantProfile profile, int score, DateTime startTime, DateTime endTime)
        {
            long start = startTime.ToUnixTime();
            if (!tournamentsProgresses.tournaments.TryGetValue(start, out TournamentProgress tournamentProgress))
            {
                tournamentProgress = new TournamentProgress(tournamentsProgresses);
                tournamentsProgresses.tournaments[start] = tournamentProgress;
            }
            
            if (!tournamentProgress.playerScores.TryGetValue(playerId, out StoredPlayerScore lastStoredScore))
            {
                lastStoredScore = new StoredPlayerScore(tournamentProgress)
                {
                    name = profile.name, 
                    score = 0, 
                    normalizedTime = 0f, 
                    coefficientValue = UnityEngine.Random.Range(0.5f, 2f)
                };
            }

            tournamentProgress.playerScores[playerId] = new StoredPlayerScore(tournamentProgress)
            {
                name = profile.name,
                score = score, 
                normalizedTime = TimeUtils.GetCurrentTimeSpentRatio(startTime, endTime),
                coefficientValue = lastStoredScore.coefficientValue
            };

            Save();
        }
        
        public void IncrementScore(string playerId, ParticipantProfile profile, int amount, DateTime startTime, DateTime endTime)
        {
            int currentScore = GetStoredScore(playerId, startTime)?.score ?? 0;
            SetScore(playerId, profile, currentScore + amount, startTime, endTime);
        }

        public void IncrementPlayerScore(int amount, ParticipantProfile profile, DateTime startTime, DateTime endTime)
            => IncrementScore(PLAYER_ID, profile, amount, startTime, endTime);

        public TournamentProgress GetLastOutdatedScores(DateTime currentStartTime)
        {
            var orderedPastTournaments = tournamentsProgresses.tournaments.Where(kvp => kvp.Key < currentStartTime.ToUnixTime()).OrderBy(kvp => kvp.Key).ToList();
            if (orderedPastTournaments.Any())
                return orderedPastTournaments.Last().Value;
            return null;
        }

        public TournamentProgress GetTournament(StoredPlayerScore storedPlayerScore)
        {
            foreach (TournamentProgress tournamentProgress in tournamentsProgresses.tournaments.Values)
            {
                foreach (var score in tournamentProgress.playerScores.Values)
                {
                    if (score == storedPlayerScore)
                        return tournamentProgress;
                }
            }
            throw new Exception("Couldn't find given stored score.");
        }
        
        public DateTime GetStartTime(TournamentProgress tournamentProgress)
        {
            foreach (var kvp in tournamentsProgresses.tournaments)
            {
                if (kvp.Value == tournamentProgress)
                    return TimeUtils.FromUnixTime(kvp.Key);
            }
            throw new Exception("Couldn't find given stored tournament progress.");
        }

        public bool HasOutdatedScores(DateTime currentStartTime)
        {
            return tournamentsProgresses.tournaments.Any(kvp => kvp.Key < currentStartTime.ToUnixTime());
        }

        public void ClearPastScores(DateTime currentStartTime)
        {
            tournamentsProgresses.tournaments = tournamentsProgresses.tournaments.Where(kvp => kvp.Key == currentStartTime.ToUnixTime()).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            Save();
        }

    }
}
