using System;

namespace LionStudios.Suite.Leaderboards.Fake
{
    public static class StatUtils
    {
        static double CalculateZScoreInNormalDistrib(double p)
        {
            // Coefficients in rational approximations
            double[] a =
            {
                -3.969683028665376e+01, 2.209460984245205e+02,
                -2.759285104469687e+02, 1.383577518672690e+02,
                -3.066479806614716e+01, 2.506628277459239e+00
            };

            double[] b =
            {
                -5.447609879822406e+01, 1.615858368580409e+02,
                -1.556989798598866e+02, 6.680131188771972e+01,
                -1.328068155288572e+01
            };

            double[] c =
            {
                -7.784894002430293e-03, -3.223964580411365e-01,
                -2.400758277161838e+00, -2.549732539343734e+00,
                4.374664141464968e+00, 2.938163982698783e+00
            };

            double[] d =
            {
                7.784695709041462e-03, 3.224671290700398e-01,
                2.445134137142996e+00, 3.754408661907416e+00
            };

            // Define break-points
            double pLow = 0.02425;
            double pHigh = 1 - pLow;

            // Rational approximation for lower region
            if (p < pLow)
            {
                double q = Math.Sqrt(-2 * Math.Log(p));
                return (((((c[0] * q + c[1]) * q + c[2]) * q + c[3]) * q + c[4]) * q + c[5]) /
                       ((((d[0] * q + d[1]) * q + d[2]) * q + d[3]) * q + 1);
            }

            // Rational approximation for upper region
            if (p > pHigh)
            {
                double q = Math.Sqrt(-2 * Math.Log(1 - p));
                return -(((((c[0] * q + c[1]) * q + c[2]) * q + c[3]) * q + c[4]) * q + c[5]) /
                       ((((d[0] * q + d[1]) * q + d[2]) * q + d[3]) * q + 1);
            }

            // Rational approximation for central region
            if (p >= pLow && p <= pHigh)
            {
                double q = p - 0.5;
                double r = q * q;
                return (((((a[0] * r + a[1]) * r + a[2]) * r + a[3]) * r + a[4]) * r + a[5]) * q /
                       (((((b[0] * r + b[1]) * r + b[2]) * r + b[3]) * r + b[4]) * r + 1);
            }

            // If p-value is out of range, return an error indicator
            return double.NaN;
        }

        public static string OrdinalSuffix(int ordinal)
        {
            //Because negatives won't work with modular division as expected:
            var abs = Math.Abs(ordinal);

            var lastdigit = abs % 10;

            return
                //Catch 60% of cases (to infinity) in the first conditional:
                lastdigit > 3 || lastdigit == 0 || (abs % 100) - lastdigit == 10 ? "th"
                : lastdigit == 1 ? "st"
                : lastdigit == 2 ? "nd"
                : "rd";
        }
    }
}