using UnityEngine;

namespace ParkourDelivery.Gameplay
{
    /// <summary>
    /// Follows the target closely horizontally. Vertically it only moves once the
    /// target leaves a dead zone, and then more smoothly, so small jumps do not
    /// shake the view.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [Tooltip("Camera position relative to the target; positive x shows more of what is ahead.")]
        [SerializeField] private Vector2 offset = new Vector2(5f, 3f);
        [SerializeField] private float horizontalSmoothTime = 0.08f;
        [SerializeField] private float verticalSmoothTime = 0.35f;
        [Tooltip("How far the target may move up or down before the camera follows.")]
        [SerializeField] private float verticalDeadZone = 3f;
        [Tooltip("The camera never looks lower than this, so the street below stays out of view.")]
        [SerializeField] private float minY = -2f;

        private float focusY;
        private float velocityX;
        private float velocityY;

        public Transform Target
        {
            get => target;
            set
            {
                target = value;
                SnapToTarget();
            }
        }

        private void Start()
        {
            SnapToTarget();
        }

        private void LateUpdate()
        {
            if (target == null) return;

            float desiredY = target.position.y + offset.y;
            if (desiredY > focusY + verticalDeadZone) focusY = desiredY - verticalDeadZone;
            else if (desiredY < focusY - verticalDeadZone) focusY = desiredY + verticalDeadZone;

            var position = transform.position;
            position.x = Mathf.SmoothDamp(position.x, target.position.x + offset.x, ref velocityX, horizontalSmoothTime);
            position.y = Mathf.SmoothDamp(position.y, Mathf.Max(focusY, minY), ref velocityY, verticalSmoothTime);
            transform.position = position;
        }

        private void SnapToTarget()
        {
            if (target == null) return;
            focusY = target.position.y + offset.y;
            transform.position = new Vector3(target.position.x + offset.x, Mathf.Max(focusY, minY), transform.position.z);
        }
    }
}
