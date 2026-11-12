using UnityEngine;

namespace ParkourDelivery.Gameplay
{
    internal static class PlayerDetector
    {
        public static bool IsPlayer(Component other)
        {
            return other != null && other.GetComponentInParent<PlayerController>() != null;
        }
    }
}
