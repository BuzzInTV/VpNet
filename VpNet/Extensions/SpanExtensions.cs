using System;
using System.Text;

namespace VpNet.Extensions
{
    internal static class SpanExtensions
    {
        public static double ToDouble(this ReadOnlySpan<byte> value)
        {
            Span<char> chars = stackalloc char[value.Length];
            Encoding.UTF8.GetChars(value, chars);
            return double.Parse(chars);
        }

        public static int ToInt32(this ReadOnlySpan<byte> value)
        {
            Span<char> chars = stackalloc char[value.Length];
            Encoding.UTF8.GetChars(value, chars);
            return int.Parse(chars);
        }

        public static float ToSingle(this ReadOnlySpan<byte> value)
        {
            Span<char> chars = stackalloc char[value.Length];
            Encoding.UTF8.GetChars(value, chars);
            return float.Parse(chars);
        }
    }
}
