using Artigio.MVVMToolkit.Core.Services.Accessibility.TextResize;
using UnityEngine;
using VContainer;

namespace Artigio.MVVMToolkit.Core.MVVM.Base.Decorators
{
    [RequireComponent(typeof(BaseViewModel))]
    public class Resizable : MonoBehaviour, IResizable
    {

        [Inject] private ITextResizeService _textResizeService;

        private IViewModel _viewModel;
        private const string ResizedUssClassName = "resized";


        private void Start()
        {
            if (_viewModel == null && TryGetComponent(out _viewModel)) 
                _textResizeService.RegisterResizableTextObject(this);
            
        }

        private void OnDisable()
        {
            if(_viewModel != null)
                _textResizeService.UnregisterResizableTextObject(this);
        }

        public void Resize(bool maximized) => _viewModel?.Container?.EnableInClassList(ResizedUssClassName, maximized);
    }
}