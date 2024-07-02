using System;
using System.Threading;
using System.Threading.Tasks;
using LionStudios.Suite.Core.LeanTween;
using UnityEngine;
using UnityEngine.UI;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public class RewardBoxDisplay : MonoBehaviour
    {
        [SerializeField] private Image iconImg;
        [SerializeField] private GameObject rotatingLights;

        [Header("Rotating Light Parameters")]
        public float rotationSpeed = 10f;
        public float scaleDuration = 1f;
        public Vector3 minScale = new Vector3(0.8f, 0.8f, 0.8f);
        public Vector3 maxScale = new Vector3(1.2f, 1.2f, 1.2f);

        private RankRewardsDisplay popupRankRewardsDisplay;

        private static CancellationTokenSource popupCancellationTokenSource = new CancellationTokenSource();

        private RankRewards rankRewards;
        
        public void Init(RankRewards rankRewards, bool animateRotatingLights)
        {
            this.rankRewards = rankRewards;
            popupRankRewardsDisplay = GetComponentInParent<LeagueLeaderboardScreen>()?.rewardsPopup;
            iconImg.sprite = rankRewards.boxSprite;
            
            if (this.GetComponent<Button>() != null && popupRankRewardsDisplay != null)
            {
                this.GetComponent<Button>().onClick.AddListener(() => ShowUnboxedRewardsPopup(rankRewards));
            }

            if(rotatingLights != null)
            {
                rotatingLights.SetActive(animateRotatingLights);
                if(animateRotatingLights)
                {
                    LeanTween.rotateAroundLocal(rotatingLights, Vector3.forward, 360f, 360f / rotationSpeed)
                         .setEase(LeanTweenType.linear)
                         .setLoopClamp();


                    ScaleUpAndDown();
                }
            }
        }

        void ScaleUpAndDown()
        {
            LeanTween.scale(rotatingLights, maxScale, scaleDuration)
                     .setEase(LeanTweenType.easeInOutSine)
                     .setLoopPingPong();
        }

        async void ShowUnboxedRewardsPopup(RankRewards rankRewards)
        {
            rotatingLights?.SetActive(false);
            popupCancellationTokenSource.Cancel(false);
            popupCancellationTokenSource = new CancellationTokenSource();
            popupRankRewardsDisplay.transform.parent.gameObject.SetActive(true);
            popupRankRewardsDisplay.transform.parent.position = transform.TransformPoint(new Vector3(this.GetComponent<RectTransform>().rect.center.x, this.GetComponent<RectTransform>().rect.max.y));
            popupRankRewardsDisplay.Init(rankRewards, false);
            try
            {
                await Task.Delay(1500, popupCancellationTokenSource.Token);                
            }
            catch (OperationCanceledException) { }
            popupRankRewardsDisplay.transform.parent.gameObject.SetActive(false);
        }

        internal void OpenChest()
        {
            iconImg.sprite = rankRewards.openedBoxSprite;
        }
        
    }
}
