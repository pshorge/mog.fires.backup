using System;
using System.Collections;
using UnityEngine;
using VContainer;

namespace Artigio.MVVMToolkit.Core.Application.Services
{
    public interface IInactivityService
    {
        event Action OnInactivityDetected;
        bool IsMonitoring { get; }
        void StartMonitoring();
        void StopMonitoring();
        void ResetTimer();
    }

    public record InactivityServiceSettings(int timeout);

    
    public sealed class InactivityService : MonoBehaviour, IInactivityService
    {
        [SerializeField] private float checkInterval = 0.5f;
        private float _timeoutDuration;
        private float _lastActivityTime;
        private bool _isMonitoring;
        private Coroutine _activityCheckCoroutine;
        
        public event Action OnInactivityDetected;
        public bool IsMonitoring => _isMonitoring;

        [Inject]
        public void Initialize(InactivityServiceSettings settings)
        {
            _timeoutDuration = settings.timeout;
        }
        
        private IEnumerator ActivityCheckRoutine()
        {
            var wait = new WaitForSeconds(checkInterval);
            while (_isMonitoring)
            {
                if (Time.time - _lastActivityTime > _timeoutDuration)
                {
                    OnInactivityDetected?.Invoke();
                    ResetTimer();
                }
                yield return wait;
            }
        }

        public void StartMonitoring()
        {
            if (_activityCheckCoroutine != null) return;
    
            _isMonitoring = true;
            ResetTimer();
            _activityCheckCoroutine = StartCoroutine(ActivityCheckRoutine());
        }

        public void StopMonitoring()
        {
            if (_activityCheckCoroutine == null) return;
    
            StopCoroutine(_activityCheckCoroutine);
            _activityCheckCoroutine = null;
            _isMonitoring = false;
        }

        public void ResetTimer() => _lastActivityTime = Time.time;

        private void Update()
        {
            if (Input.anyKeyDown) 
                ResetTimer();
        }
    }
}