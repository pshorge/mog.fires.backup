using System;
using Sources.Infrastructure.Input.Actions;

namespace Sources.Infrastructure.Input.Abstractions
{
    public interface IInputSource
    {
        event Action<InputActionType> ActionTriggered;
        string SourceName { get; }
        bool IsEnabled { get; set; }
    }
}