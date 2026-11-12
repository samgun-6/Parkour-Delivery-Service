using ParkourDelivery.Core;
using UnityEngine;

namespace ParkourDelivery.Gameplay
{
    /// <summary>
    /// Costs the player a life on contact: traps, birds, and the kill zone below the level.
    /// Works with trigger and solid colliders.
    /// </summary>
    public class Hazard : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            Hit(other);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            Hit(collision.collider);
        }

        private static void Hit(Component other)
        {
            if (PlayerDetector.IsPlayer(other) && GameManager.Instance != null)
            {
                GameManager.Instance.PlayerHit();
            }
        }
    }
}
