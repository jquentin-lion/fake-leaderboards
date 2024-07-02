using TMPro;
using UnityEngine;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public class LeagueInfoScreen : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI step01Lbl;
        [SerializeField] private TextMeshProUGUI step02Lbl;
        [SerializeField] private TextMeshProUGUI step03Lbl;


        private void Init()
        {
            string customCollectionTxt = "Trophies";
            if (LeaguesManager.CustomInfoCollectionTxt != null)
            {
                customCollectionTxt = LeaguesManager.CustomInfoCollectionTxt?.Invoke(true);
            }

            step01Lbl.text = $"Collect <color=\"yellow\"> {customCollectionTxt} </color> to progress in the League";
            step02Lbl.text = $"Reach to the top to earn <color=\"yellow\">Epic Prizes!</color>";
            step03Lbl.text = $"Finish in the <color=\"yellow\"> Top  {LeaguesManager.Instance.promoteCount} </color> to be <color=\"green\">promoted</color>  to the next League!";
        }

        public void Show()
        {
            Init();
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}