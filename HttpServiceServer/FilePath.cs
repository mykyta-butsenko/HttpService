using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HttpServiceServer
{
    internal static class FilePath
    {
        private const string FilesDir = "Files";
        private const string IndexPage = "Index.html";
        private const string ErrorPage = "Error.html";

        private static readonly string FilesDirPath =
            Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), FilesDir);

        public static string IndexPagePath => Path.Join(FilesDirPath, IndexPage);

        public static string ErrorPagePath => Path.Join(FilesDirPath, ErrorPage);
    }
}
