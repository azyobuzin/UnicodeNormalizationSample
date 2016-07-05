using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

public static class Utils
{
    public static uint[] ToUtf32Array(this string source)
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

    public static uint ParseCodePoint(string s)
    {
        return uint.Parse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
    }

    public static uint[] ParseCodePointRange(string s)
    {
        var match = Regex.Match(s, @"([0-9A-Fa-f]+)(?:\.\.([0-9A-Fa-f]+))?");
        var start = ParseCodePoint(match.Groups[1].Value);
        if (!match.Groups[2].Success) return new[] { start };
        var end = ParseCodePoint(match.Groups[2].Value);
        var result = new uint[end - start + 1];
        for (var i = start; i <= end; i++) result[i - start] = i;
        return result;
    }

    public static bool ArrEq<T>(this T[] x, T[] y)
    {
        if (x.Length != y.Length) return false;

        var comp = EqualityComparer<T>.Default;
        for (var i = 0; i < x.Length; i++)
        {
            if (!comp.Equals(x[i], y[i]))
                return false;
        }

        return true;
    }
}
