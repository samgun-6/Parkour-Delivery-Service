using UnityEngine;

namespace ParkourDelivery.Gameplay
{
    /// <summary>
    /// Flies back and forth between its start position and an offset, bobbing up and down.
    /// Pair with a Hazard and a trigger collider.
    /// </summary>
    public class Bird : MonoBehaviour
    {
        [SerializeField] private Vector2 travel = new Vector2(8f, 0f);
        [SerializeField] private float speed = 3f;
        [SerializeField] private float bobHeight = 0.4f;
        [SerializeField] private float bobSpeed = 4f;

        private Vector3 start;
        private float phase;

        private void Start()
        {
            start = transform.position;
        }

        private void Update()
        {
            float distance = travel.magnitude;
            if (distance < 0.01f) return;
            phase += Time.deltaTime * speed / distance;
            float t = Mathf.PingPong(phase, 1f);
            var position = start + (Vector3)(travel * t);
            position.y += Mathf.Sin(Time.time * bobSpeed) * bobHeight;

            // Face the direction of travel.
            float direction = Mathf.Sign(travel.x) * (Mathf.Repeat(phase, 2f) < 1f ? 1f : -1f);
            var scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (direction >= 0 ? 1f : -1f);
            transform.localScale = scale;
            transform.position = position;
        }
    }
}
