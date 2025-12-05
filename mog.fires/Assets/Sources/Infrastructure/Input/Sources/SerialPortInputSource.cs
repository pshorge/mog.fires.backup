using System;
using System.Collections.Generic;
using Sources.Infrastructure.Configuration;
using Sources.Infrastructure.Input.Abstractions;
using Sources.Infrastructure.Input.Actions;
using VContainer.Unity;

namespace Sources.Infrastructure.Input.Sources
{
    public interface ISerialPortReceiver
    {
        void OnButtonPressed(int buttonId);
    }
    
    public class SerialPortInputSource : IInputSource, ISerialPortReceiver
    {
        public event Action<InputActionType> ActionTriggered;
        public string SourceName => "SerialPort";
        public bool IsEnabled { get; set; } = true;
        
        private readonly Dictionary<int, InputActionType> _mappings = new();
        
        public SerialPortInputSource(AppConfig config)
        {
            foreach (var binding in config.Input.Serial)
            {
                _mappings[binding.ButtonId] = binding.Action;
            }
        }
       
        public void OnButtonPressed(int buttonId)
        {
            if (!IsEnabled) return;
            
            if (_mappings.TryGetValue(buttonId, out var action))
            {
                ActionTriggered?.Invoke(action);
            }
        }
    }
}