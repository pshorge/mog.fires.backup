using System;
using Sources.Infrastructure.Input.Abstractions;
using Sources.Infrastructure.Input.Actions;
using VContainer.Unity;

namespace Sources.App
{
    public class QuitOnEscapeHandler : IStartable, IDisposable
    {
        private readonly IUnifiedInputService _inputService;
        private IDisposable _subscription;

        public QuitOnEscapeHandler(IUnifiedInputService inputService)
        {
            _inputService = inputService;
        }

        public void Start()
        {
            _subscription = _inputService.Subscribe(InputActionType.QuitApp, QuitGame);
        }

        private void QuitGame()
        {
#if !UNITY_EDITOR
            UnityEngine.Application.Quit();
#endif
        }

        public void Dispose()
        {
            _subscription?.Dispose();
        }
    }
}