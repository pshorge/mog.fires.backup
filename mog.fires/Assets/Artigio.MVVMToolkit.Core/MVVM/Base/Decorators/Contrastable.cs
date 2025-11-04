using Artigio.MVVMToolkit.Core.Services.Accessibility.HighContrast;
using UnityEngine;
using VContainer;

namespace Artigio.MVVMToolkit.Core.MVVM.Base.Decorators
{
    [RequireComponent(typeof(BaseViewModel))]
    public class Contrastable : MonoBehaviour, IContrastable
    {
        
        [Inject] private IHighContrastService _highContrastService;

        private IViewModel _viewModel;
        private const string ContrastUssClassName = "contrast";

        private void Start()
        {
            if (_viewModel == null && TryGetComponent(out _viewModel)) 
                _highContrastService.RegisterHighContrastObject(this);
            
        }

        private void OnDisable()
        {
            if(_viewModel != null)
                _highContrastService.UnregisterHighContrastObject(this);
        }

        public void UpdateContrast(bool state) => _viewModel?.Container?.EnableInClassList(ContrastUssClassName,state);
    }
}