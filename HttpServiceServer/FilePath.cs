using System.Reflection;

namespace HttpServiceServer
{
    internal static class FilePath
    {
        private const string FilesDir = "Files";
        private const string IndexPage = "Index.html";
        public const string Favicon = "favicon.ico";

        private static readonly string FilesDirPath =
            Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), FilesDir);

        public static string IndexPagePath => Path.Join(FilesDirPath, IndexPage);

        public static string FaviconPath => Path.Combine(FilesDirPath, Favicon);
    }
}
