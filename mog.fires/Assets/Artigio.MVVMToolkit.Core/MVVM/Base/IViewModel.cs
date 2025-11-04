using UnityEngine.UIElements;

namespace Artigio.MVVMToolkit.Core.MVVM.Base
{
    public interface IViewModel<out TView> : IViewTypeProvider<TView>, IViewModel
    {
        bool IsVisible { get; }
        void Show();
        void Hide();
    }

    public interface IViewModel
    {
        VisualElement Container { get; }
    }
}