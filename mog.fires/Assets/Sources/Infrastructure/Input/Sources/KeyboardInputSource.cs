using System;
using System.Collections.Generic;
using Sources.Infrastructure.Input.Abstractions;
using Sources.Infrastructure.Input.Actions;
using Sources.Infrastructure.Input.Mappings;
using UnityEngine;
using VContainer.Unity;

namespace Sources.Infrastructure.Input.Sources
{
    public class KeyboardInputSource : IInputSource, ITickable
    {
        public event Action<InputActionType> ActionTriggered;
        public string SourceName => "Keyboard";
        public bool IsEnabled { get; set; } = true;
        
        private readonly Dictionary<KeyCode, InputActionType> _mappings;
        
        public KeyboardInputSource(InputMappingConfig config)
        {
            _mappings = config.GetKeyboardDictionary();
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