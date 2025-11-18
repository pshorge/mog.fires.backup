using System;
using System.Collections.Generic;

namespace Psh.MVPToolkit.Core.Infrastructure.Caching
{
    /// <summary>
    /// A generic Least Recently Used (LRU) cache implementation.
    /// Stores key-value pairs with a fixed capacity, evicting the least recently used item when full.
    /// Optionally supports a cleanup action for values being removed or replaced.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the cache.</typeparam>
    /// <typeparam name="TValue">The type of the values in the cache. Must be a reference type.</typeparam>
    /// <example>
    /// // Example 1: Using with Texture2D in Unity, with cleanup
    /// var textureCache = new LRUCache<string, Texture2D>(
    ///     capacity: 10,
    ///     cleanupAction: texture => Object.Destroy(texture)
    /// );
    /// textureCache.Add("key1", new Texture2D(128, 128));
    /// var texture = textureCache.Get("key1");
    ///
    /// // Example 2: Using with strings, no cleanup needed
    /// var stringCache = new LRUCache<string, string>(capacity: 10);
    /// stringCache.Add("key1", "value1");
    /// var value = stringCache.Get("key1");
    /// </example>
    public class LRUCache<TKey, TValue> where TValue : class
    {
        private readonly int _capacity;
        private readonly Dictionary<TKey, LinkedListNode<(TKey key, TValue value)>> _cache;
        private readonly LinkedList<(TKey key, TValue value)> _lruList;
        private readonly Action<TValue> _cleanupAction;

        public LRUCache(int capacity, Action<TValue> cleanupAction = null)
        {
            _capacity = capacity;
            _cache = new Dictionary<TKey, LinkedListNode<(TKey key, TValue value)>>();
            _lruList = new LinkedList<(TKey key, TValue value)>();
            _cleanupAction = cleanupAction;
        }

        public TValue Get(TKey key)
        {
            if (_cache.TryGetValue(key, out var node))
            {
                _lruList.Remove(node);
                _lruList.AddFirst(node);
                return node.Value.value;
            }
            return null;
        }

        public void Add(TKey key, TValue value)
        {
            if (_cache.TryGetValue(key, out var val))
            {
                _lruList.Remove(val);
                _cleanupAction?.Invoke(val.Value.value); 
            }
            else if (_cache.Count >= _capacity)
            {
                var last = _lruList.Last.Value;
                _cache.Remove(last.key);
                _lruList.RemoveLast();
                _cleanupAction?.Invoke(last.value); 
            }

            var node = new LinkedListNode<(TKey key, TValue value)>((key, value));
            _lruList.AddFirst(node);
            _cache[key] = node;
        }
        
        public bool Remove(TKey key)
        {
            if (!_cache.TryGetValue(key, out var node)) 
                return false;
            _cleanupAction?.Invoke(node.Value.value);
            _lruList.Remove(node);
            return _cache.Remove(key);
        }

        public void Clear()
        {
            foreach (var item in _cache.Values)
            {
                _cleanupAction?.Invoke(item.Value.value); 
            }
            _cache.Clear();
            _lruList.Clear();
        }
    }
}