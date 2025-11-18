using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Psh.MVPToolkit.Core.Application.Services
{
    public class ContentUpdatingService : MonoBehaviour
    {
        private VisualElement _root;
        private VisualElement _loadingProgressBar;
        private Label _loadingPercentageText;

        private float _endWidth; // max width
        private readonly string _manifestFileName = "files.txt";

        void Start()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;
            _loadingProgressBar = _root.Q<VisualElement>("bar_Progress");
            _loadingPercentageText = _root.Q<Label>("txt_Percentage");
            RunAfterUIRendered(() => {
                _endWidth = _loadingProgressBar.parent.worldBound.width - 25;
                StartCoroutine(UnityEngine.Application.isEditor ? PerformFakeAnimation(): UpdateContentAndCopyFiles());
            });
        }

        private IEnumerator UpdateContentAndCopyFiles()
        {
            string manifestPath = Path.Combine(UnityEngine.Application.streamingAssetsPath, _manifestFileName);

            var manifestRequest = UnityWebRequest.Get(manifestPath);
            yield return manifestRequest.SendWebRequest();
            if (manifestRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Cant read manifest {_manifestFileName}: " + manifestRequest.error);
                yield break;
            }

            string manifestContent = manifestRequest.downloadHandler.text;
            string[] files = manifestContent.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                                             .Select(f => f.Trim())
                                             .Where(f => !string.IsNullOrEmpty(f))
                                             .ToArray();

            int totalFiles = files.Length;
            if (totalFiles == 0)
            {
                Debug.LogWarning("No files were found!");
                OnLoadingComplete();
                yield break;
            }

            // Tworzymy set z manifestu do szybkiej weryfikacji
            var manifestSet = files.ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Usuwamy pliki z persistentDataPath/content/, których nie ma w manifest.txt
            string contentDir = Path.Combine(UnityEngine.Application.persistentDataPath, "content");
            if (Directory.Exists(contentDir))
            {
                var existingFiles = Directory.GetFiles(contentDir, "*", SearchOption.AllDirectories);
                foreach (var ef in existingFiles)
                {
                    // Ścieżka względna wobec contentDir
                    string relativePath = ef.Replace(contentDir + Path.DirectorySeparatorChar, "").Replace('\\','/');
                    if (!manifestSet.Contains(relativePath))
                    {
                        Debug.Log($"Removing stale file: {ef}");
                        File.Delete(ef);
                    }
                }
            }

            // Teraz kopiujemy aktualne pliki
            for (int i = 0; i < totalFiles; i++)
            {
                string fileName = files[i];
                yield return StartCoroutine(CopySingleFile(fileName));

                float progress = ((float)(i + 1) / totalFiles) * 100f;
                SetProgress(progress);
            }

            OnLoadingComplete();
        }

        private void SetProgress(float percentage)
        {
            if (percentage < 0) percentage = 0;
            if (percentage > 100) percentage = 100;

            _loadingPercentageText.text = $"{Mathf.RoundToInt(percentage)}%";
            var newWidth = (percentage / 100f) * _endWidth;
            _loadingProgressBar.style.width = newWidth;
        }

        private IEnumerator PerformFakeAnimation()
        {
            int totalFiles = 100;
            var wait = new WaitForSeconds(0.05f);
            for (int i = 0; i < totalFiles; i++)
            {
                yield return wait;
                float progress = ((float)(i + 1) / totalFiles) * 100f;
                SetProgress(progress);
            }

            OnLoadingComplete();
        }

        private IEnumerator CopySingleFile(string fileName)
        {
            string sourcePath = Path.Combine(UnityEngine.Application.streamingAssetsPath, fileName);
            string destPath = Path.Combine(UnityEngine.Application.persistentDataPath, fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(destPath));

            UnityWebRequest request = UnityWebRequest.Get(sourcePath);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                byte[] data = request.downloadHandler.data;
                File.WriteAllBytes(destPath, data);
                Debug.Log($"Copied: {fileName} to {destPath}");
            }
            else
            {
                Debug.LogError($"Coping failed: {fileName} {request.error}");
            }
        }

        private void OnLoadingComplete()
        {
            StartCoroutine(LoadNextSceneAsync());
        }

        protected void RunAfterUIRendered(Action action)
        {
            StartCoroutine(RunAfterFrame(action));
        }

        private IEnumerator RunAfterFrame(Action action)
        {
            yield return new WaitForEndOfFrame();
            action?.Invoke();
        }

        IEnumerator LoadNextSceneAsync()
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);

            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            yield return new WaitForSeconds(1);
        }
    }
}
