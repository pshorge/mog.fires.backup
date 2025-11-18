using Psh.MVPToolkit.Core.MVP.Contracts;
using Psh.MVPToolkit.Core.Services.Accessibility.TextResize;
using UnityEngine;
using VContainer;

namespace Psh.MVPToolkit.Core.MVP.Base.Decorators
{
    [RequireComponent(typeof(BaseView))]
    public class Resizable : MonoBehaviour, IResizable
    {
        [Inject] private ITextResizeService _textResizeService;

        private IView _view;
        private const string ResizedUssClassName = "resized";

        private void Start()
        {
            if (_view == null && TryGetComponent(out _view)) 
                _textResizeService.RegisterResizableTextObject(this);
        }

        private void OnDisable()
        {
            if(_view != null)
                _textResizeService.UnregisterResizableTextObject(this);
        }

        public void Resize(bool maximized) => _view?.Container?.EnableInClassList(ResizedUssClassName, maximized);
    }
}