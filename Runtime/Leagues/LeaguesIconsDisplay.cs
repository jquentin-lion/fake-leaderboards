using System.Collections.Generic;
using LionStudios.Suite.Core.LeanTween;
using UnityEngine;
using UnityEngine.UI;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public class LeaguesIconsDisplay : MonoBehaviour
    {
        [SerializeField] private LeagueDisplay prefab;
        [SerializeField] private RectTransform trophySeparator;
        [SerializeField] private bool spawnSeparator;
        [SerializeField] private bool invertOrder;
        [SerializeField] private bool resizeBackground = false;
        [SerializeField] private float firstBackgroundWidth = 200f;
        [SerializeField] private float lastBackgroundWidth = 800f;
        [SerializeField] private bool rescaleIcon = false;
        [SerializeField] private float firstIconScale = 1f;
        [SerializeField] private float lastIconScale = 1.4f;
        [SerializeField] private int forceCurrent = -1;
        [SerializeField] private Transform initialLeaguesPosition;

        private List<LeagueDisplay> leagueDisplays = new List<LeagueDisplay>();

        public void Init(List<League> leagues, int current)
        {
            transform.DestroyChildrenImmediate();
            leagueDisplays.Clear();
            if (forceCurrent >= 0)
                current = forceCurrent;
            
            for (var i = 0; i < leagues.Count; i++)
            {
                int ind = invertOrder ? leagues.Count - i - 1 : i;
                League league = leagues[ind];
                LeagueDisplay instance = Instantiate(prefab, transform);
                if (spawnSeparator)
                { 
                    if (i < leagues.Count - 1)
                    {
                        Instantiate(trophySeparator, transform);
                    }
                }
               

                instance.Init(league, current, ind, true);
                if (resizeBackground)
                    instance.ResizeBackground(Mathf.Lerp(firstBackgroundWidth, lastBackgroundWidth, Mathf.InverseLerp(0, leagues.Count - 1, ind)));
                if (rescaleIcon)
                    instance.RescaleIcon(Mathf.Lerp(firstIconScale, lastIconScale, Mathf.InverseLerp(0, leagues.Count - 1, ind)));

                leagueDisplays.Add(instance);
            }
            if (invertOrder)
                leagueDisplays.Reverse();
        }

        public void Show()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());

            GetComponent<VerticalLayoutGroup>().enabled = false;

            for (int i = 0; i < leagueDisplays.Count; i++)
            {
                LeagueDisplay leagueDisplay = leagueDisplays[i];

                Vector3 targetPosition = leagueDisplay.gameObject.transform.position;
                leagueDisplay.transform.position = initialLeaguesPosition.position;

                LeanTween.move(leagueDisplay.gameObject, targetPosition, 1.5f).setEase(LeanTweenType.easeInOutBack).setDelay(i * 0.2f);
            }
        }
    }
}