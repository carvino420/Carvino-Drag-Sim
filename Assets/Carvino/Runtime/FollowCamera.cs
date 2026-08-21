using UnityEngine;

namespace Carvino
{
    public sealed class FollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 3.2f, -8.5f);
        private readonly Vector3[] offsets =
        {
            new Vector3(0f, 3.2f, -8.5f),
            new Vector3(0f, 6.2f, -13.5f),
            new Vector3(6.5f, 3.2f, -8.5f),
            new Vector3(0f, 1.65f, -5.1f)
        };
        private readonly float[] fieldsOfView = { 60f, 54f, 63f, 68f };
        private Camera cameraComponent;
        private int mode;

        public string ModeName => new[] { "CHASE", "BROADCAST", "THREE-QUARTER", "LOW LAUNCH" }[mode];

        private void Awake() => cameraComponent = GetComponent<Camera>();

        public void SetTarget(Transform newTarget) => target = newTarget;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.JoystickButton4))
            {
                mode = (mode + 1) % offsets.Length;
                offset = offsets[mode];
                if (cameraComponent != null) cameraComponent.fieldOfView = fieldsOfView[mode];
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;
            float distanceMultiplier = SettingsController.CameraStyle == 1 ? 0.78f : SettingsController.CameraStyle == 2 ? 1.24f : 1f;
            Vector3 adjustedOffset = new Vector3(offset.x * distanceMultiplier, offset.y * Mathf.Lerp(0.86f, 1.08f, distanceMultiplier), offset.z * distanceMultiplier);
            transform.position = Vector3.Lerp(transform.position, target.position + adjustedOffset, Time.deltaTime * 4f);
            float lookAhead = mode == 3 ? 6f : 9f;
            transform.LookAt(target.position + new Vector3(0f, 0.8f, lookAhead));
        }
    }
}
