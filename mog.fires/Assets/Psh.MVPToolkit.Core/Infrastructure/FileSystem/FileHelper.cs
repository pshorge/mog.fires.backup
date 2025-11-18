using System;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace Psh.MVPToolkit.Core.Infrastructure.FileSystem
{
    public static class FileHelper
    {

        
        
        public static async UniTask<string> ReadAllTextFromFileAsync(string fullPath)
        {
            try
            {
                
                return await File.ReadAllTextAsync(fullPath);
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to load data file at path: {fullPath}, Error: {e.Message}\n{e.StackTrace}");
            }
        }
        
        public static string ReadAllTextFromFile(string fullPath)
        {
            try
            {
                return File.ReadAllText(fullPath);
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to load data file at path: {fullPath}, Error: {e.Message}\n{e.StackTrace}");
            }
        }
        
        public static async UniTask<string> ReadAllTextFromZippedFileAsync(string fullPath)
        {
            using var request = UnityWebRequest.Get(fullPath);
            await request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
                return request.downloadHandler.text;

            throw new Exception($"Failed to load data file at path: {fullPath}, Error: {request.error}");
        }
        

        public static string ReadAllTextFromZippedFile(string fullPath)
        {

            return ReadAllTextFromZippedFileAsync(fullPath).GetAwaiter().GetResult();
        }
        
        public static async UniTask WriteAllTextToFileAsync(string fullPath, string data, bool overwrite = false)
        {
            try
            {
                if(!File.Exists(fullPath) || overwrite)
                    await File.WriteAllTextAsync(fullPath, data);
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to write data file at path: {fullPath}, Error: {e.Message}\n{e.StackTrace}");
            }
        }
        
        public static void WriteAllTextToFile(string fullPath, string data, bool overwrite = false)
        {
            try
            {
                if(!File.Exists(fullPath) || overwrite)
                    File.WriteAllText(fullPath, data);
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to write data file at path: {fullPath}, Error: {e.Message}\n{e.StackTrace}");
            }
        }
        
        
        
    }
}