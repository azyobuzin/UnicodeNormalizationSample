using System.Collections.Generic;
using System.Linq;

namespace Decomposition
{
    using static Constants; // ハングル関連の定数をインポート

    public class Decomposer
    {
        private readonly IReadOnlyDictionary<uint, DecompositionMapping> _decompositionTable;
        private readonly IReadOnlyDictionary<uint, int> _canonicalCombiningClassTable;

        public Decomposer(UnicodeDataRecord[] data)
        {
            // 分解テーブル
            _decompositionTable = data.Where(x => x.DecompositionMapping != null)
                .ToDictionary(x => x.CodePoint, x => x.DecompositionMapping);

            // CCCテーブル
            _canonicalCombiningClassTable = data.Where(x => x.CanonicalCombiningClass != 0)
                .ToDictionary(x => x.CodePoint, x => x.CanonicalCombiningClass);
        }

        /// <summary>正規分解を行います。</summary>
        /// <param name="input">UTF-32でエンコードされた文字列。</param>
        /// <param name="compatibility">互換分解するかどうか。</param>
        public uint[] Decompose(uint[] input, bool compatibility)
        {
            var result = new List<uint>(input.Length * 2);

            // 分解
            foreach (var c in result)
                DecomposeCore(c, result, compatibility);

            // 並べ替え
            Reorder(result);

            return result.ToArray();
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

        private int GetCanonicalCombiningClass(uint c)
        {
            int ccc;
            return _canonicalCombiningClassTable.TryGetValue(c, out ccc) ? ccc : 0;
        }

        /// <summary>正規順序に並べ替えます。</summary>
        /// <param name="target">正規分解されたUTF-32文字列</param>
        private void Reorder(List<uint> target)
        {
            for (var i = 1; i < target.Count; i++)
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

                        // CCCが 0 だったら並べ替え終了
                        if (jccc == 0 || jccc <= ccc)
                            break;

                        target[j + 1] = jc;
                        target[j] = c;
                    }
                }
            }
        }
    }
}
