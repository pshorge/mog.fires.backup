using Psh.MVPToolkit.Core.Navigation;
using Sources.Presentation.Core.Types;

namespace Sources.Presentation.Navigation
{
    public class NavigationState : INavigationState<ViewType>
    {
        public ViewType ViewType { get; }
        public bool CanNavigateFrom { get; }
        public bool RequiresAuthentication { get; }
        public object NavigationArgs { get; }
        
        public NavigationState(ViewType viewType, object args = null, 
            bool canNavigateFrom = true, bool requiresAuth = false)
        {
            ViewType = viewType;
            NavigationArgs = args;
            CanNavigateFrom = canNavigateFrom;
            RequiresAuthentication = requiresAuth;
        }
    }
}