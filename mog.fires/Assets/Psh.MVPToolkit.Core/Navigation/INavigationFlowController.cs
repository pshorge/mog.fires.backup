
namespace Psh.MVPToolkit.Core.Navigation
{
    public interface INavigationFlowController<in TViewType> where TViewType : struct
    {
        void NavigateTo(TViewType viewType);
        void NavigateTo<TArgs>(TViewType viewType, TArgs args) where TArgs : class;
        void NavigateForward();
        void NavigateBack();
        void SetButtonsVisibility(bool isVisible);
        void RefreshUI();
    }
}