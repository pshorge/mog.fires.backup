using UnityEngine;
using VContainer.Unity;

namespace Sources.App
{
    public class QuitOnEscapeHandler : IInitializable, ITickable
    {
        public void Initialize()
        {
            Debug.Log("[QuitOnEscape] Registered – press ESC to quit");
        }

        public void Tick()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                QuitGame();
            }
        }

        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }
    }
}