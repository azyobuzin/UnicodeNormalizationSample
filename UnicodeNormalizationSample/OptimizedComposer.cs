using System.Collections.Generic;
using System.Linq;

namespace UnicodeNormalizationSample
{
    // 合成・分解共通の処理は Normalizer.cs へ
    partial class Normalizer
    {
        /// <summary>合成のクイックチェック用CCCテーブルを作成します。</summary>
        /// <param name="unicodeData">UnicodeData.txt の内容</param>
        /// <param name="normalizationProps">DerivedNormalizationProps.txt の内容</param>
        /// <returns>Key: コードポイント, Value: CCC または、 NFC_QCがNかMなら255、NFKC_QCがNかMなら256</returns>
        private static IReadOnlyDictionary<uint, int> CreateCompositionQuickCheckTable(UnicodeDataRecord[] unicodeData, NormalizationProps normalizationProps)
        {
            return unicodeData.Where(x => x.CanonicalCombiningClass != 0).Where(x => !normalizationProps.NfcQC.ContainsKey(x.CodePoint)).Select(x => new { x.CodePoint, Ccc = x.CanonicalCombiningClass }) // CCC
                .Concat(normalizationProps.NfcQC.Select(x => new { CodePoint = x.Key, Ccc = 255 })) // NFC_QC
                .Concat(normalizationProps.NfkcQC.Where(x => !normalizationProps.NfcQC.ContainsKey(x.Key)).Select(x => new { CodePoint = x.Key, Ccc = 256 })) // NFKC_QC
                .ToDictionary(x => x.CodePoint, x => x.Ccc);
        }

        /// <summary>正規合成を行います。</summary>
        /// <param name="input">UTF-32でエンコードされた文字列。</param>
        /// <param name="compatibility">互換分解するかどうか。</param>
        public uint[] ComposeOptimized(uint[] input, bool compatibility)
        {
            var startIndex = IndexToStartCompose(input, 0, compatibility);

            if (startIndex == input.Length) return input; // 正規化済み

            var buffer = new List<uint>(input.Length * 2);
            buffer.AddRange(input.Take(startIndex));

            do
            {
                var nextQcYes = FindNextQcYes(input, startIndex + 1, compatibility);
                var countBeforeDecomposition = buffer.Count;

                DecomposeInRange(input, startIndex, nextQcYes, buffer, compatibility);
                ComposeInRange(buffer, countBeforeDecomposition);

                if (nextQcYes == input.Length) break; // 最後まで処理した

                startIndex = IndexToStartCompose(input, nextQcYes, compatibility);

                var len = startIndex - nextQcYes;
                if (len > 0)
                {
                    buffer.AddRange(input.Skip(nextQcYes).Take(len));
                    // 配列の範囲指定して List<T> に突っ込ませろや！！！
                }
            } while (startIndex < input.Length);

            return buffer.ToArray();
        }

        /// <summary>
        /// 正規化を開始するべきインデックスを取得します。
        /// 通常は、クイックチェックの結果がNやMになる1つ前のインデックスを返します。
        /// </summary>
        private int IndexToStartCompose(uint[] input, int startIndex, bool compatibility)
        {
            var indexOfLastCccZero = startIndex;
            var lastCcc = 0;
            for (var i = startIndex; i < input.Length; i++)
            {
                int ccc;
                if (_compositionQuickCheckTable.TryGetValue(input[i], out ccc))
                {
                    if (ccc == 255)
                    {
                        return indexOfLastCccZero; // NFC_QC = N or M
                    }
                    else if (ccc == 256)
                    {
                        if (compatibility)
                            return indexOfLastCccZero; // NFKC_QC = N or M

                        indexOfLastCccZero = i;
                        lastCcc = 0;
                    }
                    else
                    {
                        if (lastCcc > ccc)
                            return indexOfLastCccZero; // 正規順序違反

                        lastCcc = ccc;
                    }
                }
                else
                {
                    indexOfLastCccZero = i;
                    lastCcc = 0;
                }
            }

            return input.Length;
        }

        /// <summary>正規化を終了するべきインデックスを取得します。</summary>
        private int FindNextQcYes(uint[] input, int startIndex, bool compatibility)
        {
            // CCC = 0 かつ QC = Y を見つけたら、そのインデックスを返す
            for (var i = startIndex; i < input.Length; i++)
            {
                int ccc;
                if (!_compositionQuickCheckTable.TryGetValue(input[i], out ccc) || (!compatibility && ccc == 256))
                    return i;
            }

            return input.Length;
        }

        /// <summary>
        /// 範囲を指定して分解を行い、 <paramref name="dest"/> に <see cref="List{T}.Add(T)"/> します。
        /// </summary>
        private void DecomposeInRange(uint[] input, int startIndex, int endIndex, List<uint> dest, bool compatibility)
        {
            for (var i = startIndex; i < endIndex; i++)
                DecomposeCoreOptimized(input[i], dest, compatibility);
        }

        private void DecomposeCoreOptimized(uint c, List<uint> dest, bool compatibility)
        {
            // ハングルは分解しても合成されるだけなので分解しない、ショートカット
            if (c >= SBase && c < SBase + SCount)
            {
                dest.Add(c);
                return;
            }

            DecompositionMapping mapping;
            if (_decompositionTable.TryGetValue(c, out mapping))
            {
                // 正規分解ならば Decomposition_Type が含まれていたらパス
                if (compatibility || mapping.Type == null)
                {
                    foreach (var x in mapping.Mapping)
                        DecomposeCoreOptimized(x, dest, compatibility);

                    return;
                }
            }

            // これ以上分解できないので、 dest に追加
            // ここで正規順序並べ替えもしておく

            var insertIndex = dest.Count;

            if (insertIndex > 0)
            {
                var ccc = GetCanonicalCombiningClass(c);

                if (ccc > 0)
                {
                    for (var i = insertIndex - 1; i >= 0; i--)
                    {
                        var ic = dest[i];
                        var iccc = GetCanonicalCombiningClass(ic);

                        if (iccc <= ccc) break;

                        insertIndex--;
                    }
                }
            }

            dest.Insert(insertIndex, c);
        }

        /// <summary>
        /// <paramref name="startIndex"/> から <paramref name="buffer"/> の最後までの範囲で合成を行います。
        /// </summary>
        private void ComposeInRange(List<uint> buffer, int startIndex)
        {
            var lastStarter = default(uint);
            var lastStarterIndex = default(int);

            var inputIndex = startIndex;

            if (startIndex == 0)
            {
                // 最初の starter を探す
                while (true)
                {
                    var c = buffer[inputIndex];
                    if (GetCanonicalCombiningClass(c) == 0)
                    {
                        lastStarter = c;
                        lastStarterIndex = inputIndex++;
                        break;
                    }

                    if (++inputIndex >= buffer.Count) return; // 合成できないので終了
                }
            }

            var insertIndex = inputIndex;
            var lastChar = lastStarter;
            var lastCcc = 0;

            for (; inputIndex < buffer.Count; inputIndex++)
            {
                var c = buffer[inputIndex];

                // ハングル
                var LIndex = lastChar - LBase;
                if (LIndex >= 0 && LIndex < LCount)
                {
                    var VIndex = c - VBase;
                    if (VIndex >= 0 && VIndex < VCount)
                    {
                        lastChar = SBase + (LIndex * VCount + VIndex) * TCount;
                        buffer[insertIndex - 1] = lastChar;
                        lastCcc = 0;
                        continue;
                    }
                }

                var SIndex = lastChar - SBase;
                if (SIndex >= 0 && SIndex < SCount && (SIndex % TCount) == 0)
                {
                    var TIndex = c - TBase;
                    if (0 < TIndex && TIndex < TCount)
                    {
                        lastChar += TIndex;
                        buffer[insertIndex - 1] = lastChar;
                        lastCcc = 0;
                        continue;
                    }
                }

                // ここから通常の合成
                var ccc = GetCanonicalCombiningClass(c);

                if (ccc == 0 ? lastCcc != 0 : lastCcc >= ccc)
                {
                    // ccc が 0 のとき → lastCcc != 0 ならばブロックされている
                    // ccc が 0 でないとき → lastCcc >= ccc ならばブロックされている
                    buffer[insertIndex++] = c;
                    lastChar = c;
                    continue;
                }

                uint composed;
                if (_compositionTable.TryGetValue(new CodePointPair(lastStarter, c), out composed))
                {
                    buffer[lastStarterIndex] = composed;
                    lastStarter = composed;
                }
                else
                {
                    if (ccc == 0)
                    {
                        lastStarter = c;
                        lastStarterIndex = insertIndex;
                    }
                    buffer[insertIndex++] = c;
                    lastChar = c;
                    lastCcc = ccc;
                }
            }

            // 本当は Count プロパティ書き換えたい
            buffer.RemoveRange(insertIndex, inputIndex - insertIndex);
        }
    }
}
