using System;
using System.Collections.Generic;
using Sources.Infrastructure.Configuration;
using Sources.Infrastructure.Input.Abstractions;
using Sources.Infrastructure.Input.Actions;
using UnityEngine;
using VContainer.Unity;

namespace Sources.Infrastructure.Input.Sources
{
    public class KeyboardInputSource : IInputSource, ITickable
    {
        public event Action<InputActionType> ActionTriggered;
        public string SourceName => "Keyboard";
        public bool IsEnabled { get; set; } = true;
        
        private readonly Dictionary<KeyCode, InputActionType> _mappings = new();
        
        public KeyboardInputSource(AppConfig config)
        {
            // parsing toml names : 'L' => 'KeyCode.L' etc.
            ParseMappings(config.Input.Keyboard);
        }
        
        private void ParseMappings(List<KeyBindingConfig> bindings)
        {
            foreach (var binding in bindings)
            {
                if (Enum.TryParse<KeyCode>(binding.Key, true, out var keyCode))
                {
                    if (!_mappings.ContainsKey(keyCode))
                    {
                        _mappings[keyCode] = binding.Action;
                    }
                }
                else
                {
                    Debug.LogWarning($"[KeyboardInput] Invalid KeyCode in config: {binding.Key}");
                }
            }
        }
        
        public void Tick()
        {
            if (!IsEnabled) return;
            
            foreach (var (key, action) in _mappings)
            {
                if (UnityEngine.Input.GetKeyDown(key))
                {
                    ActionTriggered?.Invoke(action);
                }
            }
        }
    }
}