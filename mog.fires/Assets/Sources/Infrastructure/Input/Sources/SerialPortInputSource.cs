using System;
using System.Collections.Generic;
using Sources.Infrastructure.Input.Abstractions;
using Sources.Infrastructure.Input.Actions;
using Sources.Infrastructure.Input.Mappings;

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
        
        private readonly Dictionary<int, InputActionType> _mappings;
        
        public SerialPortInputSource(InputMappingConfig config)
        {
            _mappings = config.GetSerialDictionary();
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