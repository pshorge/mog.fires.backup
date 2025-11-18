using Cysharp.Threading.Tasks;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace Psh.MVPToolkit.Core.Infrastructure.Caching
{
    public interface ITextureAssetService
    {
        UniTask<Texture2D> LoadTextureAsync(string filePath, TextureCacheStrategy strategy = TextureCacheStrategy.RC);
        bool ReleaseTexture(string filePath);
        void ClearLRUCache();
        void SetLRUCacheCapacity(int capacity);
    }
}