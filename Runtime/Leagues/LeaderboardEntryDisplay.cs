using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public class LeaderboardEntryDisplay : MonoBehaviour
    {

        [SerializeField] private Image backgroundImg;
        [SerializeField] private TMP_Text rankLbl;
        [SerializeField] private Image rankImg;
        [SerializeField] private Sprite[] rankSprites;
        [SerializeField] private Image iconImg;
        [SerializeField] private TMP_Text nameLbl;
        [SerializeField] private TMP_Text scoreLbl;
        
        [SerializeField] private Sprite playerBgSprite;
        [SerializeField] private Color playerLblColor = new Color(0.8f, 0.7f, 0f);

        [SerializeField] private RankRewardsDisplay rewardsDisplay;

        private Sprite _normalBgSprite;
        private Color _normalRankLblColor;
        private Color _normalNameLbkColor;

        private bool firstInit = true;
        private Canvas _parentCanvas;

        internal int _rank;
        internal bool _isPlayer;
        
        public void Init(int rank, ParticipantData participantData, bool isPlayer)
        {
            if (_parentCanvas == null)
            {
                _parentCanvas = transform.parent.GetComponentInParent<Canvas>();
            }
            
            if (firstInit)
            {
                _normalBgSprite = backgroundImg.sprite;
                if (rankLbl != null)
                    _normalRankLblColor = rankLbl.color;
                _normalNameLbkColor = nameLbl.color;
            }
            else
            {
                backgroundImg.sprite = _normalBgSprite;
                if (rankLbl != null)
                    rankLbl.color = _normalRankLblColor;
                nameLbl.color = _normalNameLbkColor;
            }

            firstInit = false;
            
            UpdateData(rank, participantData, isPlayer);
        }

        public void UpdateData(int rank, ParticipantData participantData, bool isPlayer)
        {
            _rank = rank;

            if (isPlayer)
            {
                _isPlayer = true;
                
                if (playerBgSprite != null)
                    backgroundImg.sprite = playerBgSprite;
                if (rankLbl != null)
                    rankLbl.color = playerLblColor;
                nameLbl.color = playerLblColor;
            }
            else
            {
                _isPlayer = false;
                
                backgroundImg.sprite = _normalBgSprite;
                if (rankLbl != null)
                    rankLbl.color = _normalRankLblColor;
                nameLbl.color = _normalNameLbkColor;
            }
            
            if (rankImg != null && rank < rankSprites.Length || rankLbl == null)
            {
                rankImg.gameObject.SetActive(true);
                if (rankLbl != null)
                    rankLbl.gameObject.SetActive(false);
                rankImg.sprite = rankSprites[rank];
            }
            else
            {
                rankImg.gameObject.SetActive(false);
                rankLbl.gameObject.SetActive(true);
                rankLbl.text = (rank + 1).ToString();
            }
            if (iconImg != null)
                iconImg.sprite = participantData.icon;
            nameLbl.text = participantData.name;
            scoreLbl.text = participantData.score.ToString();
            
            if (rewardsDisplay != null)
            {
                LeaguesManager leaguesManager = LeaguesManager.Instance;

                RankRewards rankRewards = leaguesManager.leagues[leaguesManager.CurrentLeague].GetRankRewardsCopy(rank);
                rewardsDisplay.Init(rankRewards, true);
            }
        }

        internal void PutThisOnTopOfSortingOrder()
        {
            Canvas canvas = GetComponent<Canvas>();

            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
            }

            canvas.overrideSorting = true;
            canvas.sortingOrder = _parentCanvas.sortingOrder + 1;
        }

        internal void ResetSortingOrder()
        {
            Canvas canvas = GetComponent<Canvas>();

            if (canvas != null)
            {
                Destroy(canvas);
            }
        }
        
    }
}
