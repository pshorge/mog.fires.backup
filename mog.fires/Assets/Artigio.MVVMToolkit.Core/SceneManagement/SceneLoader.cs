using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Artigio.MVVMToolkit.Core.SceneManagement
{
    

    public class SceneLoader : MonoBehaviour, ISceneLoader
    {
        private AsyncOperation _currentLoadOperation;
        private AsyncOperation _currentUnloadOperation;
        
        public async UniTask LoadSceneAdditively(int sceneIndex)
        {
            
            await UniTask.SwitchToMainThread();
            
            if (_currentLoadOperation != null)
            {
                Debug.LogWarning($"Load operation for scene '{sceneIndex}' is already in progress.");
                return;
            }
            
            if (SceneManager.GetSceneByBuildIndex(sceneIndex).isLoaded)
            {
                Debug.LogWarning($"Scene '{sceneIndex}' is already loaded.");
                return;
            }
            
            Debug.Log($"Starting to load scene additively: {sceneIndex}");
            _currentLoadOperation = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);
            await _currentLoadOperation;
            
            _currentLoadOperation = null; 
            Debug.Log($"Scene loaded: {sceneIndex}");
         
        }
       
        public async UniTask UnloadSceneAsync(int sceneIndex)
        {
            
            await UniTask.SwitchToMainThread();
            
            if (_currentUnloadOperation != null)
            {
                Debug.LogWarning($"Unload operation for scene '{sceneIndex}' is already in progress.");
                return;
            }
            
            if (!SceneManager.GetSceneByBuildIndex(sceneIndex).isLoaded)
            {
                Debug.LogWarning($"Scene '{sceneIndex}' is not loaded.");
                return;
            }

            Debug.Log($"Starting to unload scene: {sceneIndex}");
            _currentUnloadOperation = SceneManager.UnloadSceneAsync(sceneIndex);
            await _currentUnloadOperation;

            _currentUnloadOperation = null; 
            Debug.Log($"Scene unloaded: {sceneIndex}");
        }

        
    }
}