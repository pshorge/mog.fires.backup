using Sources.Infrastructure.Configuration;
using UnityEngine;
using VContainer;

namespace Sources.Infrastructure
{
     public class EarthController : MonoBehaviour
     {
        [SerializeField] private Transform cameraPivot;

        private float _yawSensitivity = 0.12f;
        private float _pitchSensitivity = 0.1f;
        private float _damping = 0.97f;

        private bool _invertYaw = false;
        private bool _invertPitch = true;
        private float _minPitch = -85f;
        private float _maxPitch = 85f;

        private float currentYaw;
        private float currentPitch;
        private float yawVelocity;
        private float pitchVelocity;
        private Camera cam;
        
        public Camera Camera => cam;
        
        private bool _inputActive;

        [Inject]
        private void Construct(AppConfig config)
        {
            _yawSensitivity = config.Camera.YawSensitivity;
            _pitchSensitivity = config.Camera.PitchSensitivity;
            _damping = Mathf.Clamp(config.Camera.Damping,0,0.99f);
            _invertYaw = config.Camera.InvertYaw;
            _invertPitch = config.Camera.InvertPitch;
            _minPitch = config.Camera.MinPitch;
            _maxPitch = config.Camera.MaxPitch;
        }
        
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
        }
        
        public void SetInputActive(bool active)
        {
            _inputActive = active;
            if (active) return;
            yawVelocity = 0f;
            pitchVelocity = 0f;
        }
        

        void Update()
        {
            
            if (!_inputActive)
            {
                ApplyInertiaAndRotation();
                return;
            }
            
            float mouseX = UnityEngine.Input.GetAxis("Mouse X");
            float mouseY = UnityEngine.Input.GetAxis("Mouse Y");

            int yawDirection = _invertYaw ? -1 : 1;
            int pitchDirection = _invertPitch ? -1 : 1;

            yawVelocity += mouseX * _yawSensitivity * yawDirection;
            pitchVelocity += mouseY * _pitchSensitivity * pitchDirection;
            
            ApplyInertiaAndRotation();
        }
       
        void LateUpdate()
        {
            if (cam != null && cameraPivot != null)
            {
                cam.transform.LookAt(cameraPivot.position);
            }
        }

        
        private void ApplyInertiaAndRotation()
        {
            yawVelocity *= _damping;
            pitchVelocity *= _damping;

            currentYaw += yawVelocity * Time.deltaTime * 100f;
            currentPitch += pitchVelocity * Time.deltaTime * 100f;
            currentPitch = Mathf.Clamp(currentPitch, _minPitch, _maxPitch);
           
            Quaternion yawRotation = Quaternion.Euler(0f, currentYaw, 0f);
            Quaternion pitchRotation = Quaternion.Euler(currentPitch, 0f, 0f);
            
            //apply rotations, multiplication order matters!
            cameraPivot.rotation = yawRotation * pitchRotation;
        }
          
     }
}

