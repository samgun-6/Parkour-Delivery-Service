using UnityEngine;

namespace ParkourDelivery.Core
{
    /// <summary>
    /// Lives, packages and elapsed time for one run of a level.
    /// Plain C# so it can be unit tested without a scene.
    /// </summary>
    public class RunStats
    {
        public int Lives { get; private set; }
        public int PackagesCollected { get; private set; }
        public int PackagesTotal { get; }
        public float ElapsedSeconds { get; private set; }

        public RunStats(int lives, int packagesTotal)
        {
            Lives = lives;
            PackagesTotal = packagesTotal;
        }

        public void Tick(float deltaSeconds)
        {
            ElapsedSeconds += deltaSeconds;
        }

        public void CollectPackage()
        {
            PackagesCollected = Mathf.Min(PackagesCollected + 1, PackagesTotal);
        }

        /// <summary>Removes one life. Returns true when no lives are left.</summary>
        public bool LoseLife()
        {
            Lives = Mathf.Max(Lives - 1, 0);
            return Lives == 0;
        }
    }
}
