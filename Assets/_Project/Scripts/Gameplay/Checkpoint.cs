using ParkourDelivery.Core;
using UnityEngine;

namespace ParkourDelivery.Gameplay
{
    /// <summary>Sets where the player respawns after losing a life. Needs a trigger collider.</summary>
    [RequireComponent(typeof(Collider2D))]
    public class Checkpoint : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer flag;
        [SerializeField] private Color activeColor = new Color(0.3f, 0.9f, 0.3f);

        public bool IsActive { get; private set; }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsActive || !PlayerDetector.IsPlayer(other) || GameManager.Instance == null) return;
            IsActive = true;
            GameManager.Instance.SetCheckpoint(transform.position);
            if (flag != null) flag.color = activeColor;
        }
    }
}
