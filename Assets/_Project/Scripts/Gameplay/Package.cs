using ParkourDelivery.Core;
using UnityEngine;

namespace ParkourDelivery.Gameplay
{
    /// <summary>A package to pick up during the run. Needs a trigger collider.</summary>
    [RequireComponent(typeof(Collider2D))]
    public class Package : MonoBehaviour
    {
        [SerializeField] private float bobHeight = 0.25f;
        [SerializeField] private float bobSpeed = 2f;

        private Vector3 basePosition;

        private void Start()
        {
            basePosition = transform.position;
        }

        private void Update()
        {
            float offset = Mathf.Sin((Time.time + basePosition.x) * bobSpeed) * bobHeight;
            transform.position = basePosition + Vector3.up * offset;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!PlayerDetector.IsPlayer(other) || GameManager.Instance == null) return;
            if (GameManager.Instance.State != GameState.Playing) return;
            GameManager.Instance.CollectPackage();
            gameObject.SetActive(false);
        }
    }
}
