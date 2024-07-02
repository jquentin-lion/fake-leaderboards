using TMPro;
using UnityEngine;

namespace LionStudios.Suite.Leaderboards.Fake
{
    [RequireComponent(typeof(TMP_Text))]
    public class CurrentLeagueNameDisplay : MonoBehaviour
    {

        [SerializeField] private string suffix;

        private TMP_Text text;

        private LeaguesManager leaguesManager;

        public void Init(LeaguesManager league)
        {
            leaguesManager = league;
            text = GetComponent<TMP_Text>();
            InitializeValues();
        }

        private void InitializeValues()
        {
            League currentLeague = leaguesManager.leagues[leaguesManager.CurrentLeague];
            text.text = $"{currentLeague.name}{suffix}";
            text.color = Color.white;
            text.fontSharedMaterial = currentLeague.nameMaterial;
        }
    }
}
