using UnityEngine;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public static class ColorUtils
    {
        public static Color ApplyHDRIntensity(Color c, float intensity)
        {
            float factor = Mathf.Pow(2, intensity);
            return new Color(c.r * factor, c.g * factor, c.b * factor, c.a);
        }
    }
}
