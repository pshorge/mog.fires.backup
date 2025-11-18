using Cysharp.Threading.Tasks;
using Psh.MVPToolkit.Core.Infrastructure.Caching;
using UnityEngine;
using UnityEngine.UIElements;

namespace Psh.MVPToolkit.Core.UI
{
    public static class VisualElementExtensions
    {
        public static async UniTask<Texture2D> SetImageElementAsync(
            this VisualElement element, 
            string path,
            ITextureAssetService textureAssetService,
            bool releaseOnDestroy = true,
            TextureCacheStrategy strategy = TextureCacheStrategy.RC)
        {
            if (element == null)
            {
                Debug.LogWarning("Can not set image element: VisualElement is null!");
                return null;
            }
            
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("Can not set image element: path is null or empty");
                return null;
            }

            Texture2D texture = await textureAssetService.LoadTextureAsync(path, strategy);
            
            if (texture == null)
            {
                return null;
            }
            element.style.backgroundImage = new StyleBackground(texture);
            
            // Dla strategii RC rejestrujemy zwolnienie przy zniszczeniu
            if (releaseOnDestroy && strategy == TextureCacheStrategy.RC)
            {
                element.RegisterCallback<DetachFromPanelEvent>(_ => 
                {
                    textureAssetService.ReleaseTexture(path);
                });
            }

            return texture;
        }

        public static bool SetBackgroundFromResources(
            this VisualElement element,
            string path,
            bool releaseOnDestroy = true)
        {
            if (element == null)
            {
                Debug.LogWarning("Nie można ustawić tła: VisualElement jest null!");
                return false;
            }
    
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("Nie można ustawić tła: ścieżka jest null lub pusta!");
                return false;
            }

            Texture2D texture = Resources.Load<Texture2D>(path);
    
            if (texture == null)
            {
                Debug.LogWarning($"Nie udało się załadować tekstury ze ścieżki: {path}");
                return false;
            }
    
            element.style.backgroundImage = new StyleBackground(texture);
    
            if (releaseOnDestroy)
            {
                element.RegisterCallback<DetachFromPanelEvent>(_ => 
                {
                    Resources.UnloadAsset(texture);
                });
            }
            return true;
        }
        
    }
}