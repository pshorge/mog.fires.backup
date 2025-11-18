using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Psh.MVPToolkit.Core.SceneManagement
{
   
    public class NextSceneLoader : MonoBehaviour
    {
        IEnumerator LoadNextSceneAsync()
        {

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);

            while (!asyncLoad.isDone)
            {
                yield return null;
            }


        }

        public void LoadNextScene()
        {
            StartCoroutine(LoadNextSceneAsync());
        }
    }
}