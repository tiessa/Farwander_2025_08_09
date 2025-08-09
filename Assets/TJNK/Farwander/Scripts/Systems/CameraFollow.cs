using UnityEngine;

namespace TJNK.Farwander.Systems
{
    [DisallowMultipleComponent]
    public class CameraFollow : MonoBehaviour
    {
        [Tooltip("What the camera should follow.")]
        public Transform target;

        [Tooltip("Z offset should usually be -10 for 2D.")]
        public Vector3 offset = new Vector3(0, 0, -10);

        [Tooltip("Larger = snappier, smaller = floatier.")]
        public float followLerp = 12f;

        [Tooltip("Snap instantly the first frame after target is set.")]
        public bool snapOnFirstSet = true;

        private bool _snappedOnce;

        void LateUpdate()
        {
            if (!target) return;

            var goal = target.position + offset;

            if (snapOnFirstSet && !_snappedOnce)
            {
                transform.position = goal;
                _snappedOnce = true;
                return;
            }

            // Smoothly move towards the target
            transform.position = Vector3.Lerp(transform.position, goal, 1f - Mathf.Exp(-followLerp * Time.deltaTime));
        }

        /// <summary>Call this to instantly snap to the target once.</summary>
        public void SnapNow()
        {
            if (!target) return;
            transform.position = target.position + offset;
            _snappedOnce = true;
        }
    }
}