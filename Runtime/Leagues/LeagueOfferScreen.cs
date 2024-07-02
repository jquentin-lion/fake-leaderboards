using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public class LeagueOfferScreen : LeagueScreen
    {
        
        [SerializeField] private Button continueBtn;

        [SerializeField] private LeaguesIconsDisplay leaguesDisplay;

        public void Init(List<League> leagues,bool joinOverride, Action onContinue)
        {
            leaguesDisplay.Init(leagues, -1);
            continueBtn.onClick.RemoveAllListeners();
            if (!joinOverride)
            {
                continueBtn.onClick.AddListener(Hide);
                continueBtn.onClick.AddListener(() => onContinue?.Invoke());
            }
        }

        public override void Show()
        {
            base.Show();

            leaguesDisplay.Show();
        }

    }
}
