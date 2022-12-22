using System.Text;

namespace HttpServiceServer
{
    internal static class StringExtensions
    {
        public static string AppendNewLine(this string value)
        {
            return $"{value}{Environment.NewLine}";
        }
        public static byte[] ToBytes(this string value)
        {
            return Encoding.UTF8.GetBytes(value);
        }
    }
}
