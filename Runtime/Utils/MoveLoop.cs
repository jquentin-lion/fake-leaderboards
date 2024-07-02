using LionStudios.Suite.Core.LeanTween;
using UnityEngine;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public class MoveLoop : MonoBehaviour
    {

        [SerializeField] private Vector3 localTarget;
        [SerializeField] private float time = 1f;
        [SerializeField] private LeanTweenType easeType = LeanTweenType.easeInOutSine;

        private void Start()
        {
            gameObject.LeanMoveLocal(localTarget, time).setEase(easeType).setLoopPingPong();
        }
    }
}
