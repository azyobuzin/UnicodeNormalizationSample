using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace UnicodeNormalizationSample
{
    /// <summary>
    /// UnicodeData.txt のレコード
    /// </summary>
    public class UnicodeDataRecord
    {
        /// <summary>コードポイント</summary>
        public uint CodePoint { get; }

        /// <summary>Canonical_Combining_Class</summary>
        public int CanonicalCombiningClass { get; }

        /// <summary>
        /// Decomposition_Type, Decomposition_Mapping
        /// <para>空ならば <c>null</c> が代入されます。</para>
        /// </summary>
        public DecompositionMapping DecompositionMapping { get; }

        public UnicodeDataRecord(uint codePoint, int canonicalCombiningClass, DecompositionMapping decompositionMapping)
        {
            this.CodePoint = codePoint;
            this.CanonicalCombiningClass = canonicalCombiningClass;
            this.DecompositionMapping = decompositionMapping;
        }
    }

    public class DecompositionMapping
    {
        /// <summary>&lt;ここ&gt;</summary>
        public string Type { get; }

        /// <summary>分解後のコードポイント</summary>
        public uint[] Mapping { get; }

        public DecompositionMapping(string type, uint[] mapping)
        {
            if (mapping == null || mapping.Length == 0)
                throw new ArgumentNullException(nameof(mapping));

            this.Type = type;
            this.Mapping = mapping;
        }

        public static DecompositionMapping Parse(string s)
        {
            var mapping = s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // 空なら null
            if (mapping.Length == 0) return null;

            if (mapping[0][0] == '<')
            {
                // 互換分解
                return new DecompositionMapping(
                    mapping[0].Substring(1, mapping[0].Length - 2), // < > を削除
                    mapping.Skip(1).Select(Utils.ParseCodePoint).ToArray()
                );
            }

            // 正規分解
            return new DecompositionMapping(
                null,
                Array.ConvertAll(mapping, Utils.ParseCodePoint)
            );
        }
    }

    public static class UnicodeData
    {
        public static UnicodeDataRecord[] Load(string fileName)
        {
            var result = new List<UnicodeDataRecord>();
            using (var reader = new StreamReader(fileName))
            {
                string line;
                while (!string.IsNullOrEmpty(line = reader.ReadLine()))
                {
                    var s = line.Split(';');
                    result.Add(new UnicodeDataRecord(
                        Utils.ParseCodePoint(s[0]),
                        int.Parse(s[3], CultureInfo.InvariantCulture),
                        DecompositionMapping.Parse(s[5])
                    ));
                }
            }
            return result.ToArray();
        }
    }
}
