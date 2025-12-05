using Sources.Infrastructure.Configuration;
using UnityEngine;
using VContainer;

namespace Sources.Infrastructure
{
    public class MapController : MonoBehaviour
    {
        private Vector2 _targetPosition = new(0.5f, 0.5f);
        private Vector2 _currentPosition = new(0.5f, 0.5f);
        private Vector2 _velocity; 
        
        private bool _inputActive;

        private float _sensitivity =1.5f;
        private float _smoothTime = 0.1f;

        public Vector2 PositionNormalized => _currentPosition;

        [Inject]
        private void Construct(AppConfig config)
        {
            _sensitivity = config.Input.CursorSensitivity;
            _smoothTime = config.Input.CursorSmoothTime;
        }
        
        public void SetInputActive(bool active)
        {
            _inputActive = active;
        }

        public void ResetPosition()
        {
            _targetPosition = new Vector2(0.5f, 0.5f);
            _currentPosition = _targetPosition;
            _velocity = Vector2.zero;
        }

        private void Update()
        {
            if (_inputActive)
            {
                float inputX = UnityEngine.Input.GetAxis("Mouse X");
                float inputY = UnityEngine.Input.GetAxis("Mouse Y");
                
                _targetPosition.x += inputX * _sensitivity * Time.deltaTime;
                _targetPosition.y -= inputY * _sensitivity * Time.deltaTime;

                _targetPosition.x = Mathf.Clamp01(_targetPosition.x);
                _targetPosition.y = Mathf.Clamp01(_targetPosition.y);
            }

            _currentPosition = Vector2.SmoothDamp(_currentPosition, _targetPosition, ref _velocity, _smoothTime);
        }
    }
}