using Cysharp.Threading.Tasks;

namespace Psh.MVPToolkit.Core.SceneManagement
{
    public interface ISceneLoader
    {
        UniTask LoadSceneAdditively(int sceneIndex);
        UniTask UnloadSceneAsync(int sceneIndex);
    }
}