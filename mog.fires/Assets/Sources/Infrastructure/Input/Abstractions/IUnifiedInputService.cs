using System;
using Sources.Infrastructure.Input.Actions;

namespace Sources.Infrastructure.Input.Abstractions
{
    public interface IUnifiedInputService
    {
        event Action<InputActionType> OnAction;
        IDisposable Subscribe(InputActionType action, Action callback);
        void SetSourceEnabled(string sourceName, bool enabled);
    }
}