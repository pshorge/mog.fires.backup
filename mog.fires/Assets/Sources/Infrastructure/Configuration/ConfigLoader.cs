using System.IO;
using Psh.MVPToolkit.Core.Infrastructure.FileSystem;
using Tomlyn;
using UnityEngine;
using Sources.Infrastructure.Input.Actions;

namespace Sources.Infrastructure.Configuration
{
    public static class ConfigLoader
    {
        private const string FileName = "config.toml";

        public static AppConfig Load()
        {
            string path =  ContentPathResolver.ResolveContentPath(FileName);
           

            if (!File.Exists(path))
            {
                Debug.LogWarning($"[ConfigLoader] File not found at: {path}. Using defaults.");
                return CreateDefaultConfig();
            }

            try
            {
                string content = File.ReadAllText(path);

                var options = new TomlModelOptions
                {
                    //(PascalCase ==> PascalCase)
                    ConvertPropertyName = name => name, 
                    IgnoreMissingProperties = true 
                };

                
                var config = Toml.ToModel<AppConfig>(content, options: options);
                return config ?? CreateDefaultConfig();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ConfigLoader] Failed to parse TOML: {e.Message}");
                return CreateDefaultConfig();
            }
        }

        private static AppConfig CreateDefaultConfig() => new AppConfig();

    }
}
