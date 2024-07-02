using UnityEngine;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public class ParticipantData
    {
        public ParticipantProfile profile;
        public string name => profile.name;
        public Sprite icon => profile.icon;
        public int score;

        //Maximum score allowed in current Race/Tournament/League in which this participant is participating
        private readonly int _totalScore;

        //Progress value will be between 0 and 1. Can be used for sliders in UI
        public float ProgressValue
        {
            get
            {
                if (_totalScore <= 0 || score > _totalScore)
                {
                    return 0;
                }

                return (float) score / _totalScore;
            }
        }

        public ParticipantData(int score, ParticipantProfile profile, int totalScore)
        {
            this.score = score;
            this.profile = profile;
            this._totalScore = totalScore;
        }
    }
}