using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

// ReSharper disable InconsistentNaming

namespace Psh.MVPToolkit.Core.Infrastructure.Caching
{
    public enum TextureCacheStrategy
    {
        RC,
        LRU
    }

    public sealed class TextureAssetService : ITextureAssetService
    {
        private readonly Dictionary<string, ThreadSafeRC<Texture2D>> TextureRCs = new();
        private readonly LRUCache<string, Texture2D> TextureLRU;
        private readonly object ManagerLock = new();
        private readonly Dictionary<string, TextureCacheStrategy> TextureStrategyMap = new();
        
        private const int DefaultLRUCapacity = 10;
        private int _lruCapacity = DefaultLRUCapacity;
        
        public TextureAssetService()
        {
            TextureLRU = new LRUCache<string, Texture2D>(_lruCapacity, Object.Destroy);
        }
        
        // ReSharper disable once InconsistentNaming
        public void SetLRUCacheCapacity(int capacity)
        {
            if (capacity <= 0) return;
            
            lock (ManagerLock)
            {
                _lruCapacity = capacity;
            }
        }
        
        public async UniTask<Texture2D> LoadTextureAsync(string filePath, TextureCacheStrategy strategy = TextureCacheStrategy.RC)
        {
            string cacheKey = filePath;
            
            lock (ManagerLock)
            {
                TextureStrategyMap[cacheKey] = strategy;
                
                // Jeśli używamy LRU, sprawdź najpierw czy tekstura jest w cache
                if (strategy == TextureCacheStrategy.LRU)
                {
                    var cachedTexture = TextureLRU.Get(cacheKey);
                    if (cachedTexture != null)
                    {
                        return cachedTexture;
                    }
                }
            }
            
            // Dla strategii ReferenceCount użyj istniejącego mechanizmu RC
            if (strategy == TextureCacheStrategy.RC)
            {
                ThreadSafeRC<Texture2D> rc;
                
                lock (ManagerLock)
                {
                    if (!TextureRCs.TryGetValue(cacheKey, out rc))
                    {
                        rc = new ThreadSafeRC<Texture2D>(
                            cacheKey,
                            _ => LoadTextureFromDiskAsync(filePath),
                            Object.Destroy
                        );
                        TextureRCs[cacheKey] = rc;
                    }
                }
                return await rc.Acquire();
            }
            // Dla strategii LRU, ładujemy teksturę i dodajemy do cache LRU

            var texture = await LoadTextureFromDiskAsync(filePath);

            if (texture == null) 
                return texture;
            
            lock (ManagerLock)
            {
                TextureLRU.Add(cacheKey, texture);
            }
            return texture;
        }

        public bool ReleaseTexture(string filePath)
        {
            string cacheKey = filePath;
            
            lock (ManagerLock)
            {
                // Sprawdź, jakiej strategii używamy dla tej tekstury
                if (TextureStrategyMap.TryGetValue(cacheKey, out var strategy))
                {
                    // Dla RC zwolnij referencję
                    if (strategy == TextureCacheStrategy.RC)
                    {
                        if (TextureRCs.TryGetValue(cacheKey, out var rc))
                        {
                            bool released = rc.Release();
                            if (released)
                            {
                                TextureRCs.Remove(cacheKey);
                                TextureStrategyMap.Remove(cacheKey);
                            }
                            return released;
                        }
                    }
                    // Dla LRU nie musimy nic robić - cache sam zarządza pamięcią
                    else
                    {
                        TextureLRU.Remove(cacheKey); 
                        return true;
                    }
                }
                return false;
            }
        }

        public void ClearLRUCache()
        {
            lock (ManagerLock)
            {
                TextureLRU.Clear();
                
                // Usuń wpisy ze strategii dla LRU
                var keysToRemove = new List<string>();
                foreach (var pair in TextureStrategyMap)
                {
                    if (pair.Value == TextureCacheStrategy.LRU)
                    {
                        keysToRemove.Add(pair.Key);
                    }
                }
                
                foreach (var key in keysToRemove)
                {
                    TextureStrategyMap.Remove(key);
                }
            }
        }
        
        
        private static async UniTask<Texture2D> LoadTextureFromDiskAsync(string fullPath)
        {
            if (!File.Exists(fullPath))
            {
                Debug.LogError($"Nie znaleziono pliku tekstury: {fullPath}");
                return null;
            }
            var path = new Uri(fullPath).AbsoluteUri;
            UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture( path, true);
            await webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                var loadedTexture = DownloadHandlerTexture.GetContent(webRequest);
                if (loadedTexture != null)
                {
                    loadedTexture.wrapMode = TextureWrapMode.Clamp;
                    loadedTexture.filterMode = FilterMode.Bilinear;
                    return loadedTexture;
                }
            }
            
            Debug.LogError(webRequest.error);
            return null;
            
        }
    }
}