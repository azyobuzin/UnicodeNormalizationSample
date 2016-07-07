using System.Collections.Generic;
using System.Linq;

namespace BasicSample
{
    // 合成・分解共通の処理は Normalizer.cs へ
    partial class Normalizer
    {
        /// <summary>分解テーブルを作成します。</summary>
        /// <param name="unicodeData">UnicodeData.txt の内容</param>
        private static IReadOnlyDictionary<uint, DecompositionMapping> CreateDecompositionTable(UnicodeDataRecord[] unicodeData)
        {
            return unicodeData.Where(x => x.DecompositionMapping != null)
                .ToDictionary(x => x.CodePoint, x => x.DecompositionMapping);
        }

        /// <summary>正規分解を行います。</summary>
        /// <param name="input">UTF-32でエンコードされた文字列。</param>
        /// <param name="compatibility">互換分解するかどうか。</param>
        public uint[] Decompose(uint[] input, bool compatibility)
        {
            var buffer = new List<uint>(input.Length * 2);

            // 分解
            foreach (var c in input)
                DecomposeCore(c, buffer, compatibility);

            var result = buffer.ToArray();

            // 並べ替え
            Reorder(result);

            return result;
        }

        /// <summary>1文字を分解します。</summary>
        private void DecomposeCore(uint c, List<uint> dest, bool compatibility)
        {
            var SIndex = c - SBase;
            if (SIndex >= 0 && SIndex < SCount)
            {
                // ハングル
                var L = LBase + SIndex / NCount;
                var V = VBase + (SIndex % NCount) / TCount;
                var T = TBase + SIndex % TCount;
                dest.Add(L);
                dest.Add(V);
                if (T != TBase) dest.Add(T);
            }
            else
            {
                DecompositionMapping mapping;
                if (_decompositionTable.TryGetValue(c, out mapping))
                {
                    // 正規分解ならば Decomposition_Type が含まれていたらパス
                    if (compatibility || mapping.Type == null)
                    {
                        foreach (var x in mapping.Mapping)
                            DecomposeCore(x, dest, compatibility);

                        return;
                    }
                }

                // 分解できなければそのまま
                dest.Add(c);
            }
        }

        /// <summary>正規順序に並べ替えます。</summary>
        /// <param name="target">正規分解されたUTF-32文字列</param>
        private void Reorder(uint[] target)
        {
            for (var i = 1; i < target.Length; i++)
            {
                var c = target[i];
                var ccc = GetCanonicalCombiningClass(c);

                if (ccc > 0)
                {
                    // CCCが 0 ではないので並べ替え開始
                    for (var j = i - 1; j >= 0; j--)
                    {
                        var jc = target[j];
                        var jccc = GetCanonicalCombiningClass(jc);

                        if (jccc <= ccc) break;

                        target[j + 1] = jc;
                        target[j] = c;
                    }
                }
            }
        }
    }
}
