using TMPro;
using UnityEngine;
using Button = UnityEngine.UI.Button;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public class LeagueLogsDisplay : MonoBehaviour
    {
        private const float DELAY_BETWEEN_CLICK = 0.5f;
        private const int NUMBER_CLICKS = 10;

        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private Button button;
        [SerializeField] private Button scoreButton;

        private float lastClick;
        private int clicks;
        private LeaguesManager leaguesManager;
        
        private void Awake()
        {
            leaguesManager = GetComponentInParent<LeaguesManager>();
            button.onClick.AddListener(OnClick);
            scoreButton.onClick.AddListener(Score);
            gameObject.SetActive(false);
        }

        private void OnClick()
        {
            if (Time.realtimeSinceStartup < lastClick + DELAY_BETWEEN_CLICK)
            {
                clicks++;
            }
            else
            {
                clicks = 1;
            }
            lastClick = Time.realtimeSinceStartup;
            if (clicks >= NUMBER_CLICKS)
            {
                gameObject.SetActive(!gameObject.activeSelf);
                clicks = 0;
            }
        }

        void Score()
        {
            leaguesManager.Score(1);
        }
        
        void Update()
        {
            label.text = leaguesManager.Leaderboard.logs;
        }
    }
}
