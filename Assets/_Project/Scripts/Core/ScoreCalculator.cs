using UnityEngine;

namespace ParkourDelivery.Core
{
    /// <summary>
    /// Score for a finished run: every delivered package is worth a fixed amount,
    /// and finishing faster than the level's par time adds a time bonus.
    /// </summary>
    public static class ScoreCalculator
    {
        public const int PointsPerPackage = 1000;
        public const int PointsPerSecondUnderPar = 20;

        public static int Calculate(int packagesCollected, float elapsedSeconds, float parSeconds)
        {
            int packagePoints = packagesCollected * PointsPerPackage;
            float secondsUnderPar = Mathf.Max(0f, parSeconds - elapsedSeconds);
            int timeBonus = Mathf.RoundToInt(secondsUnderPar * PointsPerSecondUnderPar);
            return packagePoints + timeBonus;
        }
    }
}
