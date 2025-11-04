using static System.IO.Path;

namespace Artigio.MVVMToolkit.Core.Infrastructure.FileSystem
{
    public static class ContentPathResolver
    {
        private static readonly string ContentPath = Combine(ContentPathProvider.ExtDataPath,"content" );
        
        public static string ResolveContentPath(string filename)
        {
            return string.IsNullOrEmpty(filename) ? null : Combine(ContentPath, filename);
        }

        public static string ResolveResourcesPath(string resourceName) => $"Images/{resourceName}";
    }
}