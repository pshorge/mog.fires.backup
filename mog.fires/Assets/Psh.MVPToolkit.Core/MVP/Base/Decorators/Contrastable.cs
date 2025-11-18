using Psh.MVPToolkit.Core.MVP.Contracts;
using Psh.MVPToolkit.Core.Services.Accessibility.HighContrast;
using UnityEngine;
using VContainer;

namespace Psh.MVPToolkit.Core.MVP.Base.Decorators
{
    [RequireComponent(typeof(BaseView))]
    public class Contrastable : MonoBehaviour, IContrastable
    {
        [Inject] private IHighContrastService _highContrastService;

        private IView _view;
        private const string ContrastUssClassName = "contrast";

        private void Start()
        {
            if (_view == null && TryGetComponent(out _view)) 
                _highContrastService.RegisterHighContrastObject(this);
        }

        private void OnDisable()
        {
            if(_view != null)
                _highContrastService.UnregisterHighContrastObject(this);
        }

        public void UpdateContrast(bool state) => _view?.Container?.EnableInClassList(ContrastUssClassName, state);
    }
}