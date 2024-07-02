using UnityEngine;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public class Rotator : MonoBehaviour
    {

        [SerializeField] private float speed = 1f;

        private void Update()
        {
            transform.Rotate(0f, 0f, speed);
        }
    }
}
