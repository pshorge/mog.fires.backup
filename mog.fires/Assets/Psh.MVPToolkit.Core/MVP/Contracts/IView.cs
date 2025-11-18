using UnityEngine.UIElements;

namespace Psh.MVPToolkit.Core.MVP.Contracts
{
    /// <summary>
    /// Base interface for MVP Views
    /// </summary>
    public interface IView<out TViewType> : IViewTypeProvider<TViewType>, IView
    {
        bool IsVisible { get; }
        void Show();
        void Hide();
    }

    public interface IView
    {
        VisualElement Container { get; }
    }
}

