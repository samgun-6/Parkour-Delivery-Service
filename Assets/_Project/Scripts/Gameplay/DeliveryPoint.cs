using ParkourDelivery.Core;
using UnityEngine;

namespace ParkourDelivery.Gameplay
{
    /// <summary>End of the run: touching it delivers the packages. Needs a trigger collider.</summary>
    [RequireComponent(typeof(Collider2D))]
    public class DeliveryPoint : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (PlayerDetector.IsPlayer(other) && GameManager.Instance != null)
            {
                GameManager.Instance.CompleteLevel();
            }
        }
    }
}
