using System;
using Sources.Infrastructure.Input.Abstractions;
using Sources.Infrastructure.Input.Actions;
using VContainer.Unity;

namespace Sources.Infrastructure.Input.Sources
{
    public class MouseInputSource : IInputSource, ITickable
    {
        public event Action<InputActionType> ActionTriggered;
        public string SourceName => "MouseButtons";
        public bool IsEnabled { get; set; } = true;

        public void Tick()
        {
            if (!IsEnabled) return;

            // LPM -> Select ('1')
            if (UnityEngine.Input.GetMouseButtonDown(0)) 
                ActionTriggered?.Invoke(InputActionType.Select);

            // PPM -> Back ('2')
            if (UnityEngine.Input.GetMouseButtonDown(1)) 
                ActionTriggered?.Invoke(InputActionType.Back);

            // SPM -> SwitchMode ('M')
            if (UnityEngine.Input.GetMouseButtonDown(2)) 
                ActionTriggered?.Invoke(InputActionType.SwitchMode);
        }
    }
}