using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicSample
{
    public partial class Normalizer
    {
        private const uint SBase = 0xAC00,
            LBase = 0x1100, VBase = 0x1161, TBase = 0x11A7,
            LCount = 19, VCount = 21, TCount = 28,
            NCount = VCount * TCount,
            SCount = LCount * NCount;

        private readonly IReadOnlyDictionary<uint, DecompositionMapping> _decompositionTable;
        private readonly IReadOnlyDictionary<uint, int> _canonicalCombiningClassTable;
        private readonly IReadOnlyDictionary<CodePointPair, uint> _compositionTable;

        public Normalizer(UnicodeDataRecord[] unicodeData, NormalizationProps normalizationProps)
        {
            // 分解テーブル
            _decompositionTable = unicodeData.Where(x => x.DecompositionMapping != null)
                .ToDictionary(x => x.CodePoint, x => x.DecompositionMapping);

            // CCCテーブル
            _canonicalCombiningClassTable = unicodeData.Where(x => x.CanonicalCombiningClass != 0)
                .ToDictionary(x => x.CodePoint, x => x.CanonicalCombiningClass);

            // 合成テーブル
            var fullCompositionExclusion = new HashSet<uint>(normalizationProps.FullCompositionExclusion);
            _compositionTable = unicodeData.Where(x => x.DecompositionMapping != null
                    && x.DecompositionMapping.Type == null // 互換分解でない
                    && x.DecompositionMapping.Mapping.Length == 2 // 1文字の分解でない
                    && !fullCompositionExclusion.Contains(x.CodePoint) // Full_Composition_Exclusion
                ).ToDictionary(
                    x => new CodePointPair(x.DecompositionMapping.Mapping[0], x.DecompositionMapping.Mapping[1]),
                    x => x.CodePoint
                );
        }

        private int GetCanonicalCombiningClass(uint c)
        {
            int ccc;
            _canonicalCombiningClassTable.TryGetValue(c, out ccc);
            return ccc;
        }
    }
}
