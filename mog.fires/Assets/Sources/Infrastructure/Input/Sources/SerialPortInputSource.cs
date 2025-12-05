using System;
using System.Collections.Generic;
using Sources.Infrastructure.Configuration;
using Sources.Infrastructure.Input.Abstractions;
using Sources.Infrastructure.Input.Actions;
using Sources.Infrastructure.Input.Services; 
using UnityEngine;
using VContainer.Unity;

namespace Sources.Infrastructure.Input.Sources
{
    public class SerialPortInputSource : IInputSource, IStartable, IDisposable
    {
        public event Action<InputActionType> ActionTriggered;
        public string SourceName => "SerialPort";
        public bool IsEnabled { get; set; } = true;

        private readonly SerialPortService _service;
        private readonly InputConfig _config;
        private readonly Dictionary<string, InputActionType> _mappings = new();
        
        // Debounce logic
        private float _lastActionTime;

        public SerialPortInputSource(AppConfig appConfig, SerialPortService service)
        {
            _config = appConfig.Input;
            _service = service;

            foreach (var binding in _config.Serial)
            {
                _mappings[binding.Message] = binding.Action;
            }
        }

        public void Start()
        {
            _service.OnMessageReceived += HandleMessage;
        }

        private void HandleMessage(string message)
        {
            if (!IsEnabled) return;

            if (Time.time - _lastActionTime < _config.DebounceTimeSeconds)
            {
                return;
            }

            if (_mappings.TryGetValue(message, out var action))
            {
                Debug.Log($"[SerialInput] Received: '{message}' -> Triggering: {action}");
                _lastActionTime = Time.time;
                ActionTriggered?.Invoke(action);
            }
            else
            {
                Debug.Log($"[SerialInput] Unknown command: '{message}'");
            }
        }

        public void Dispose()
        {
            _service.OnMessageReceived -= HandleMessage;
        }
    }
}