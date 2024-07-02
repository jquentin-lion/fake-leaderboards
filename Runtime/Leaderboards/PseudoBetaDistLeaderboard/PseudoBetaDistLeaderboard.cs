using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LionStudios.Suite.Leaderboards.Fake
{

    [Serializable]
    public class PseudoBetaDistLeaderboard : ILeaderboard<PseudoBetaDistLeaderboardData>
    {
        internal string logs;
        private PseudoBetaDistBotData[] _allBots;

        public PseudoBetaDistBotData[] GetAllBots => _allBots;

        private string[] allLoadedNames;
        
        //Data that can override too
        private int _minBotScore = 10;
        private int _maxBotScore = 100;
        private float _targetWinRatio = 0.9f;

        private int targetUpdateModulo = 14;

        private float _maxExtendedTrailRatio = 0.5f;

        public void Init(DateTime lastStartTime, int minimumBotScore, int maximumBotScore, float targetWinRatio)
        {
            OverrideLeaderboardData(minimumBotScore, maximumBotScore, targetWinRatio);
            CreateBots(lastStartTime);
        }

        internal void OverrideLeaderboardData(int minimumBotScore, int maximumBotScore, float targetWinRatio)
        {
            _minBotScore = minimumBotScore;
            _maxBotScore = maximumBotScore;
            _targetWinRatio = targetWinRatio;
        }

        private void CreateBots(DateTime lastStartTime)
        {
            _allBots = new PseudoBetaDistBotData[leaderboardData.numberOfBots];
            bool isTournamentAlreadyRunning = false;

            isTournamentAlreadyRunning =
                scoresStorage.tournamentsProgresses.tournaments.TryGetValue(lastStartTime.ToUnixTime(),
                    out TournamentProgress tournamentProgress);
            
            if(!isTournamentAlreadyRunning)
            {
                tournamentProgress = new TournamentProgress(scoresStorage.tournamentsProgresses);
                scoresStorage.tournamentsProgresses.tournaments[lastStartTime.ToUnixTime()] = tournamentProgress;
            }

            allLoadedNames = LoadAllNames();
            
            for (int i = 0; i < leaderboardData.numberOfBots; i++)
            {
                _allBots[i] = new PseudoBetaDistBotData();
                string playerId = $"Bot_{i}";
                string participantName = string.Empty;

                if (tournamentProgress.playerScores.TryGetValue(playerId, out var storedPlayerScore))
                {
                    participantName = storedPlayerScore.name;
                }
                else if(!isTournamentAlreadyRunning)
                {
                    // TODO: ensure that no 2 bots have the same name
                    SetNewBotData(ref participantName, ref tournamentProgress,playerId);
                }
                else
                {
                    if (!tournamentProgress.playerScores.TryGetValue(playerId, out var nameScore))
                    {
                        SetNewBotData(ref participantName, ref tournamentProgress,playerId);
                    }
                }
                
                _allBots[i].SetPlayerData($"Bot_{i}", new ParticipantProfile()
                {
                    name = participantName
                });
            }
        }
        
        private string[] LoadAllNames()
        {
            string[] arrayOfNames = leaderboardData.botNamesFile.text.Split('\n');
            return arrayOfNames;
        }
        
        private void SetNewBotData(ref string participantName,ref TournamentProgress tournamentProgress,string playerId)
        {
            participantName = allLoadedNames[UnityEngine.Random.Range(0, allLoadedNames.Length)];

            tournamentProgress.playerScores[playerId] = new StoredPlayerScore(tournamentProgress)
            {
                name = participantName,
                score = 0,
                normalizedTime = 0,
                coefficientValue = UnityEngine.Random.Range(0.5f, 2f)
            };
        }

        
        public LeaderboardCalculatedData GetInitialScores()
        {
            var res = new LeaderboardCalculatedData();
            res.participantDatas = _allBots.Select(b => new ParticipantData(0, b.GetProfile(), 0)).ToList();
            res.participantDatas.Add( new ParticipantData(0, GetBaseLeaderboardData().GetPlayerProfile(), 0));
            res.playerIndex = _allBots.Length;
            return res;
        }

        public override LeaderboardCalculatedData CalculatedData(DateTime startTime, DateTime endTime, DateTime currentTime, int playerScore)
        {
            logs = "";
            logs +=
                $"{_minBotScore}-{_maxBotScore}, {_targetWinRatio}, {leaderboardData.margin}\n";  
            bool store = true;
            float normalizedTime = TimeUtils.GetTimeSpentRatio(startTime, endTime, currentTime);
            logs += $"nt = {normalizedTime} ; ";
            int projectedScore = GetProjectedScore(playerScore, normalizedTime);
            logs += $"ps = {projectedScore} ; ";
            int targetValue = Mathf.Clamp(projectedScore, _minBotScore + GetCalculatedMargin, _maxBotScore - GetCalculatedMargin);
            logs += $"tv = {targetValue} ; ";
            List<int> botTargetScores = new List<int>();
            long startTimeUnix = startTime.Round().ToUnixTime();
            if (!scoresStorage.tournamentsProgresses.tournaments.TryGetValue(startTimeUnix, out TournamentProgress tournamentProgress))
            {
                tournamentProgress = new TournamentProgress(scoresStorage.tournamentsProgresses);
                if (store)
                    scoresStorage.tournamentsProgresses.tournaments[startTimeUnix] = tournamentProgress;
            }
            
            logs += $"lt = {tournamentProgress.lastTargetUpdateValue} ; ";

            // Update bots by modulo.
            // If the target value goes from 10 to 12, update bots with a mod(targetUpdateModulo) of either
            // 11 mod targetUpdateModulo, or 12 mod targetUpdateModulo.
            // This way, we update bots in a more distributed way, and not all at once.
            // We also avoid having to store info on which bots were updated last time.
            List<int> botsToUpdate = new List<int>();
            if (targetValue != tournamentProgress.lastTargetUpdateValue)
            {
                if (tournamentProgress.lastTargetUpdateValue < 0 || Mathf.Abs(projectedScore - tournamentProgress.lastTargetUpdateValue) >= targetUpdateModulo)
                {
                    botsToUpdate = Enumerable.Range(0, targetUpdateModulo).ToList();
                }
                else
                {
                    int start = Mathf.Min(projectedScore, tournamentProgress.lastTargetUpdateValue);
                    int end = Mathf.Max(projectedScore, tournamentProgress.lastTargetUpdateValue);
                    botsToUpdate = Enumerable.Range(start + 1, end - start).Select(i => i % targetUpdateModulo)
                        .Distinct().ToList();
                }

                tournamentProgress.lastTargetUpdateValue = projectedScore;
            }
            logs += $"up = {{ {string.Join(",", botsToUpdate)} }} \n";
            
            for (int i = 0; i < _allBots.Length; i++)
            {
                bool updateTarget = botsToUpdate.Contains(i % 3);
                if (updateTarget)
                    botTargetScores.Add(PseudoBetaDistBotData.GenerateBotTargetScore(_minBotScore, _maxBotScore, _targetWinRatio, targetValue, projectedScore, Mathf.RoundToInt(_maxBotScore * _maxExtendedTrailRatio)));
                else
                    botTargetScores.Add(_allBots[i].GetLastTargetScore(tournamentProgress));
            }
            
                botTargetScores.Sort();
            
            List<ParticipantData> participants = new List<ParticipantData>();
            for (var i = 0; i < _allBots.Length; i++)
            {
                PseudoBetaDistBotData bot = _allBots[i];
                logs += $"bot{i}: tg: {botTargetScores[i]} ; ";
                ParticipantData pd;
                pd = bot.GetParticipantData(tournamentProgress, botTargetScores[i], normalizedTime, store, ref logs, _allBots[i].GetProfile());
                
                logs += $"cs: {pd.score} ; ";
                participants.Add(pd);
                logs += "\n";
            }

            ParticipantData player = new ParticipantData(playerScore, leaderboardData.playerProfile, int.MaxValue); 
            participants.Add(player);
            participants = participants.OrderByDescending(p => p.score).ThenByDescending(p => (player.score == 0 ? (p != player) : (p == player))).ToList();
            int playerRank = participants.IndexOf(player);
            return new LeaderboardCalculatedData() { participantDatas = participants, playerIndex = playerRank };
        }
        
        private int GetCalculatedMargin => (int) ((_maxBotScore - _minBotScore) * (leaderboardData.margin / 100f));

    }
}
