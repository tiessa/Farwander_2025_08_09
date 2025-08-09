using UnityEngine;

namespace TJNK.Farwander.Systems
{
    [DisallowMultipleComponent]
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0, 0, -10);
        public float followLerp = 12f;
        public bool snapOnFirstSet = true;

        // NEW: keep following even when Time.timeScale == 0
        public bool useUnscaledTime = true;

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

            // use unscaled time when paused
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            transform.position = Vector3.Lerp(
                transform.position,
                goal,
                1f - Mathf.Exp(-followLerp * Mathf.Max(0f, dt))
            );
        }

        public void SnapNow()
        {
            if (!target) return;
            transform.position = target.position + offset;
            _snappedOnce = true;
        }
    }
}