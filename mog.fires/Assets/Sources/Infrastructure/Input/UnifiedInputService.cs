using System;
using System.Collections.Generic;
using System.Linq;
using Sources.Infrastructure.Input.Abstractions;
using Sources.Infrastructure.Input.Actions;

namespace Sources.Infrastructure.Input
{
    public class UnifiedInputService : IUnifiedInputService, IDisposable
    {
        public event Action<InputActionType> OnAction;
        
        private readonly Dictionary<string, IInputSource> _sources = new();
        private readonly Dictionary<InputActionType, List<Action>> _subscriptions = new();
        
        public UnifiedInputService(IEnumerable<IInputSource> sources)
        {
            foreach (var source in sources)
            {
                _sources[source.SourceName] = source;
                source.ActionTriggered += HandleAction;
            }
        }
        
        private void HandleAction(InputActionType action)
        {
            OnAction?.Invoke(action);
            
            // subscriptions per action
            if (!_subscriptions.TryGetValue(action, out var callbacks)) return;
            // copy list to avoid errors ( modifications while iterating)
            foreach (var callback in callbacks.ToList())
            {
                callback?.Invoke();
            }
        }
        
        public IDisposable Subscribe(InputActionType action, Action callback)
        {
            if (!_subscriptions.ContainsKey(action))
                _subscriptions[action] = new List<Action>();
            
            _subscriptions[action].Add(callback);
            
            return new Subscription(() => 
            {
                if (_subscriptions != null && _subscriptions.TryGetValue(action, out var list))
                {
                    list.Remove(callback);
                }
            });
        }
        
        public void SetSourceEnabled(string sourceName, bool enabled)
        {
            if (_sources.TryGetValue(sourceName, out var source))
                source.IsEnabled = enabled;
        }
        
        public void Dispose()
        {
            foreach (var source in _sources.Values)
            {
                source.ActionTriggered -= HandleAction;
            }
            _sources.Clear();
            _subscriptions.Clear();
        }
        
        private class Subscription : IDisposable
        {
            private readonly Action _unsubscribe;
            public Subscription(Action unsubscribe) => _unsubscribe = unsubscribe;
            public void Dispose() => _unsubscribe?.Invoke();
        }
    }
}