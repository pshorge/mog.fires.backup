using UnityEngine;

namespace Sources.Infrastructure
{
     public class EarthController : MonoBehaviour
     {
        [SerializeField] private Transform cameraPivot;

        [Header("Control Toggles")]
        [SerializeField] private bool isRotationEnabled = false;
        [SerializeField] private int toggleMouseButton = 0; // 0 = lewy

        [Header("Orbit Controls (Yaw & Pitch)")]
        [SerializeField] private float yawSensitivity = 0.15f;
        [SerializeField] private float pitchSensitivity = 0.15f;
        [Range(0.0f, 0.99f)]
        [SerializeField] private float damping = 0.92f;

        [Header("Directions & Limits")]
        [SerializeField] private bool invertYaw = false;
        [SerializeField] private bool invertPitch = true;
        [SerializeField] private float minPitch = -85f;
        [SerializeField] private float maxPitch = 85f;

        private float currentYaw = 0f;
        private float currentPitch = 0f;
        private float yawVelocity = 0f;
        private float pitchVelocity = 0f;
        private Camera cam;

        void Start()
        {
            if (cameraPivot == null)
            {
                cameraPivot = transform;
            }

            cam = cameraPivot.GetComponentInChildren<Camera>();
            if (cam == null)
            {
                enabled = false;
                return;
            }

            Vector3 initialAngles = cameraPivot.eulerAngles;
            currentYaw = initialAngles.y;
            currentPitch = initialAngles.x;

            UpdateCursorState();
        }

        void Update()
        {
            HandleInputToggle();
            
            if (isRotationEnabled)
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");

                int yawDirection = invertYaw ? -1 : 1;
                int pitchDirection = invertPitch ? -1 : 1;

                yawVelocity += mouseX * yawSensitivity * yawDirection;
                pitchVelocity += mouseY * pitchSensitivity * pitchDirection;
            }
            
            ApplyInertiaAndRotation();
        }

        void LateUpdate()
        {
            if (cam != null && cameraPivot != null)
            {
                cam.transform.LookAt(cameraPivot.position);
            }
        }

        private void HandleInputToggle()
        {
            if (Input.GetMouseButtonDown(toggleMouseButton))
            {
                isRotationEnabled = !isRotationEnabled;
                UpdateCursorState();
            }
        }

        private void UpdateCursorState()
        {
            Cursor.lockState = isRotationEnabled ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !isRotationEnabled;
        }

        private void ApplyInertiaAndRotation()
        {
            yawVelocity *= damping;
            pitchVelocity *= damping;

            currentYaw += yawVelocity * Time.deltaTime * 100f;
            currentPitch += pitchVelocity * Time.deltaTime * 100f;
            currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
           
            Quaternion yawRotation = Quaternion.Euler(0f, currentYaw, 0f);
            Quaternion pitchRotation = Quaternion.Euler(currentPitch, 0f, 0f);
            
            //apply rotations, multiplication order matters!
            cameraPivot.rotation = yawRotation * pitchRotation;
        }
          
     }
}

