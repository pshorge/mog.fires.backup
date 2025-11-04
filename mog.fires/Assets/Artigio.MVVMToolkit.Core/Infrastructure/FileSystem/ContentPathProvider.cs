using System.IO;
using UnityEngine;

namespace Artigio.MVVMToolkit.Core.Infrastructure.FileSystem
{
    public static class ContentPathProvider
    {
        public static string ExtDataPath
        {
            get
            {
                return UnityEngine.Application.isEditor switch
                {
                    false when UnityEngine.Application.platform == RuntimePlatform.Android => UnityEngine.Application.persistentDataPath,
                    false when UnityEngine.Application.platform != RuntimePlatform.Android => UnityEngine.Application.streamingAssetsPath,
                    _ => Path.Combine(UnityEngine.Application.dataPath, "../ExternalEditorContent")
                };
            }
        }
    }
}