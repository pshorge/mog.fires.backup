using Cysharp.Threading.Tasks;

namespace Artigio.MVVMToolkit.Core.SceneManagement
{
    public interface ISceneLoader
    {
        UniTask LoadSceneAdditively(int sceneIndex);
        UniTask UnloadSceneAsync(int sceneIndex);
    }
}