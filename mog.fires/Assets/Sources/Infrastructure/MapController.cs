using UnityEngine;

namespace Sources.Infrastructure
{
    public class MapController : MonoBehaviour
    {
        [Header("Cursor Settings")]
        [SerializeField] private float sensitivity = 1.5f; 
        [SerializeField] private float smoothTime = 0.1f; 

        private Vector2 _targetPosition = new(0.5f, 0.5f);
        private Vector2 _currentPosition = new(0.5f, 0.5f);
        private Vector2 _velocity; 
        
        private bool _inputActive;

        public Vector2 PositionNormalized => _currentPosition;

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
                float inputX = Input.GetAxis("Mouse X");
                float inputY = Input.GetAxis("Mouse Y");
                
                _targetPosition.x += inputX * sensitivity * Time.deltaTime;
                _targetPosition.y -= inputY * sensitivity * Time.deltaTime;

                _targetPosition.x = Mathf.Clamp01(_targetPosition.x);
                _targetPosition.y = Mathf.Clamp01(_targetPosition.y);
            }

            _currentPosition = Vector2.SmoothDamp(_currentPosition, _targetPosition, ref _velocity, smoothTime);
        }
    }
}