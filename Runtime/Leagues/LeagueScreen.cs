using UnityEngine;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public class LeagueScreen : MonoBehaviour
    {
        
        public virtual void Show()
        {
            gameObject.SetActive(true);
        }
        
        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }
        
    }
}
