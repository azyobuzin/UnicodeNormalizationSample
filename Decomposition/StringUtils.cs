using System;
using System.Text;

public static class StringUtils
{
    public static uint[] ToUtf32Array(string source)
    {
        var result = new uint[source.Length];
        var insertIndex = 0;
        var currentIndex = 0;
        while (currentIndex < source.Length)
        {
            var c = (uint)char.ConvertToUtf32(source, currentIndex);
            result[insertIndex++] = c;
            currentIndex += c > char.MaxValue ? 2 : 1;
        }
        Array.Resize(ref result, insertIndex);
        return result;
    }

    public static string Utf32ArrayToString(uint[] source)
    {
        var sb = new StringBuilder(source.Length * 2);
        foreach (var c in source)
        {
            if (c > char.MaxValue)
            {
                var x = c - 0x10000;
                sb.Append((char)(x / 0x400 + 0xD800))
                    .Append((char)(x % 0x400 + 0xDC00));
            }
            else
            {
                sb.Append((char)c);
            }
        }
        return sb.ToString();
    }
}
