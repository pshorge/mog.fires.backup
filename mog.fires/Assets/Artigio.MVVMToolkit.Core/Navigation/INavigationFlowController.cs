
namespace Artigio.MVVMToolkit.Core.Navigation
{
    public interface INavigationFlowController<in TViewType> where TViewType : struct
    {
        void NavigateTo(TViewType viewType);
        void NavigateTo<TArgs>(TViewType viewType, TArgs args) where TArgs : class;
        void NavigateBack();
        void NavigateForward();
        void SetButtonsVisibility(bool isVisible);
        void RefreshUI();
    }
}