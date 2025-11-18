namespace Psh.MVPToolkit.Core.Navigation
{
    public interface INavigationState<out TViewType> where TViewType : struct
    {
        TViewType ViewType { get; }
        bool CanNavigateFrom { get; }
        bool RequiresAuthentication { get; }
        object NavigationArgs { get; }
    }
}