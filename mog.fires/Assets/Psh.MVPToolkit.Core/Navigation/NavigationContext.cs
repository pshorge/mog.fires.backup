using System.Collections.Generic;

namespace Psh.MVPToolkit.Core.Navigation
{
    public class NavigationContext<TViewType> where TViewType : struct
    {
        private readonly Stack<INavigationState<TViewType>> _navigationStack = new();
        private readonly Stack<INavigationState<TViewType>> _forwardStack = new();
        
        public INavigationState<TViewType> CurrentState { get; private set; }
        public bool CanGoBack => _navigationStack.Count > 0;
        public bool CanGoForward => _forwardStack.Count > 0;
        
        public void PushState(INavigationState<TViewType> state)
        {
            if (CurrentState != null)
                _navigationStack.Push(CurrentState);
            
            CurrentState = state;
            _forwardStack.Clear();
        }
        
        public INavigationState<TViewType> PopState()
        {
            if (!CanGoBack) return null;
            
            _forwardStack.Push(CurrentState);
            CurrentState = _navigationStack.Pop();
            return CurrentState;
        }
        
        public INavigationState<TViewType> GoForward()
        {
            if (!CanGoForward) return null;
            
            _navigationStack.Push(CurrentState);
            CurrentState = _forwardStack.Pop();
            return CurrentState;
        }
        
        public void Clear()
        {
            _navigationStack.Clear();
            _forwardStack.Clear();
            CurrentState = null;
        }
    }
}