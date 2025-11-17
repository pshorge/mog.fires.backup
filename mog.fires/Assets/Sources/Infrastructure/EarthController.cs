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
          
          
          
          
          
          
          
          
          
//         [Header("Rig")]
//         [Tooltip("Pivot yaw (oś Y). Domyślnie: ten obiekt.")]
//         public Transform yawPivot;
//         [Tooltip("Pivot pitch (oś X). Dziecko yaw.")]
//         public Transform pitchPivot;
//
//         [Header("Activation")]
//         [Tooltip("Czy wymaga przycisku myszy? Dla trackballa zazwyczaj FALSE.")]
//         public bool requireMouseButton = false;
//         public int mouseButton = 0; // 0=LMB, 1=RMB, 2=MMB
//
//         [Tooltip("Zablokuj i schowaj kursor, aby ruch był nieskończony.")]
//         public bool lockAndHideCursor = true;
//         public bool lockCursorAlwaysWhenActive = true;
//         public bool unlockOnEscape = true;
//
//         [Header("Feel")]
//         [Tooltip("Czułość w deg / jednostkę Mouse X/Y.")]
//         public float speed = 240f;
//         [Tooltip("Szybkość dojścia do prędkości docelowej (1/s).")]
//         public float acceleration = 25f;
//         [Tooltip("Tłumienie inercji po puszczeniu (1/s).")]
//         public float damping = 3.5f;
//         [Tooltip("Minimalna prędkość (deg/s), poniżej której zatrzymujemy ruch.")]
//         public float minVelocity = 0.05f;
//
//         public bool invertX = false;
//         public bool invertY = false;
//
//         [Header("Limits")]
//         [Tooltip("Ograniczenie pitch (X) – aby nie wywracać Ziemi do góry nogami.")]
//         public bool clampPitch = true;
//         public float pitchMin = -89f;
//         public float pitchMax = 89f;
//
//         // Skumulowane kąty
//         private float yaw;   // wokół Y
//         private float pitch; // wokół X
//
//         // Prędkości kątowe (deg/s)
//         private float yawVel;
//         private float pitchVel;
//
//         private bool dragging;
//
//         void Awake()
//         {
//             if (!yawPivot) yawPivot = transform;
//             if (!pitchPivot)
//             {
//                 Debug.LogWarning("TrackballEarthRig: Nie przypisano pitchPivot. Upewnij się, że masz Earth_Pitch jako dziecko Earth_Yaw i przypisz je w inspektorze.");
//             }
//
//             // Odczytaj startowe kąty z pivotów (jeśli nie są zerowe)
//             yaw = NormalizeAngle(yawPivot.localEulerAngles.y);
//             if (pitchPivot)
//                 pitch = NormalizeAngle(pitchPivot.localEulerAngles.x);
//         }
//
//         void OnEnable() { UpdateCursorState(); }
//         void OnDisable() { UnlockCursor(); }
//         void OnApplicationFocus(bool focus) { if (!focus) UnlockCursor(); else UpdateCursorState(); }
//
//         void Update()
//         {
//             float dt = Time.deltaTime;
//             if (dt <= 0f) return;
//
//             if (unlockOnEscape && Input.GetKeyDown(KeyCode.Escape)) UnlockCursor();
//
//             bool wantDrag = !requireMouseButton || Input.GetMouseButton(mouseButton);
//             if (wantDrag != dragging)
//             {
//                 dragging = wantDrag;
//                 UpdateCursorState();
//             }
//
//             if (dragging)
//             {
//                 float dx = Input.GetAxisRaw("Mouse X");
//                 float dy = Input.GetAxisRaw("Mouse Y");
//                 if (invertX) dx = -dx;
//                 if (invertY) dy = -dy;
//
//                 float targetYawVel = dx * speed;
//                 float targetPitchVel = -dy * speed; // -dy daje naturalne "pchnięcie" kuli
//
//                 float t = 1f - Mathf.Exp(-Mathf.Max(0f, acceleration) * dt);
//                 yawVel = Mathf.Lerp(yawVel, targetYawVel, t);
//                 pitchVel = Mathf.Lerp(pitchVel, targetPitchVel, t);
//             }
//             else
//             {
//                 float decay = Mathf.Exp(-Mathf.Max(0f, damping) * dt);
//                 yawVel *= decay;
//                 pitchVel *= decay;
//
//                 if (Mathf.Abs(yawVel) < minVelocity) yawVel = 0f;
//                 if (Mathf.Abs(pitchVel) < minVelocity) pitchVel = 0f;
//             }
//
//             // Integracja kątów
//             yaw += yawVel * dt;
//             pitch += pitchVel * dt;
//
//             if (clampPitch) pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
//
//             // Zastosuj rotacje pivotów (brak rolla)
//             yawPivot.localRotation = Quaternion.Euler(0f, yaw, 0f);
//             if (pitchPivot)
//                 pitchPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
//         }
//
//         private void UpdateCursorState()
//         {
//             bool shouldLock = lockAndHideCursor && (dragging || (!requireMouseButton && lockCursorAlwaysWhenActive));
//             if (shouldLock) LockCursor();
//             else UnlockCursor();
//         }
//
//         private void LockCursor()
//         {
//             Cursor.lockState = CursorLockMode.Locked;
//             Cursor.visible = false;
//         }
//
//         private void UnlockCursor()
//         {
//             Cursor.lockState = CursorLockMode.None;
//             Cursor.visible = true;
//         }
//
//         private static float NormalizeAngle(float a)
//         {
//             a %= 360f;
//             if (a > 180f) a -= 360f;
//             return a;
//         }
     }
}

