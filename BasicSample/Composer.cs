using System;
using System.Collections.Generic;
using System.Linq;

namespace BasicSample
{
    // 合成・分解共通の処理は Normalizer.cs へ
    partial class Normalizer
    {
        /// <summary>合成テーブルを作成します。</summary>
        /// <param name="unicodeData">UnicodeData.txt の内容</param>
        /// <param name="normalizationProps">DerivedNormalizationProps.txt の内容</param>
        private static IReadOnlyDictionary<CodePointPair, uint> CreateCompositionTable(UnicodeDataRecord[] unicodeData, NormalizationProps normalizationProps)
        {
            var fullCompositionExclusion = new HashSet<uint>(normalizationProps.FullCompositionExclusion);
            return unicodeData.Where(x => x.DecompositionMapping != null
                    && x.DecompositionMapping.Type == null // 互換分解でない
                    && x.DecompositionMapping.Mapping.Length == 2 // 1文字の分解でない -> 2文字
                    && !fullCompositionExclusion.Contains(x.CodePoint) // Full_Composition_Exclusion
                ).ToDictionary(
                    x => new CodePointPair(x.DecompositionMapping.Mapping[0], x.DecompositionMapping.Mapping[1]),
                    x => x.CodePoint
                );
        }

        /// <summary>正規合成を行います。</summary>
        /// <param name="input">UTF-32でエンコードされた文字列。</param>
        /// <param name="compatibility">互換分解するかどうか。</param>
        public uint[] Compose(uint[] input, bool compatibility)
        {
            var buffer = Decompose(input, compatibility);

            if (buffer.Length <= 1)
                return buffer;

            var lastStarter = default(uint);
            var lastStarterIndex = default(int);

            var inputIndex = 0;

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

                if (++inputIndex >= buffer.Length)
                    return buffer; // 合成できないので終了
            }

            var insertIndex = inputIndex;
            var lastChar = lastStarter;
            var lastCcc = 0;

            for (; inputIndex < buffer.Length; inputIndex++)
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

                if (ccc != 0 && lastCcc == ccc)
                {
                    // ブロック条件 "ccc(B) >= ccc(C)"
                    // 分解で並べ替えられているので ccc(B) > ccc(C) になることはない
                    buffer[insertIndex++] = c;
                    lastChar = c;
                    continue;
                }

                uint composed;
                if ((ccc != 0 || lastCcc == 0) // ブロック条件より CCC 0 の連続は合成できる
                    && _compositionTable.TryGetValue(new CodePointPair(lastStarter, c), out composed))
                {
                    buffer[lastStarterIndex] = composed;
                    lastStarter = composed;
                    ccc = 0; // TODO: あとでこれ消してみる
                }
                else
                {
                    if (ccc == 0)
                    {
                        lastStarter = c;
                        lastStarterIndex = insertIndex;
                    }
                    buffer[insertIndex++] = c;
                }

                lastChar = c;
                lastCcc = ccc;
            }

            Array.Resize(ref buffer, insertIndex);
            return buffer;
        }
    }
}
