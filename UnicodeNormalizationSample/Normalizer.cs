using System.Collections.Generic;
using System.Linq;

namespace UnicodeNormalizationSample
{
    public partial class Normalizer
    {
        private const uint SBase = 0xAC00,
            LBase = 0x1100, VBase = 0x1161, TBase = 0x11A7,
            LCount = 19, VCount = 21, TCount = 28,
            NCount = VCount * TCount,
            SCount = LCount * NCount;

        private readonly IReadOnlyDictionary<uint, int> _canonicalCombiningClassTable;
        private readonly IReadOnlyDictionary<uint, DecompositionMapping> _decompositionTable;
        private readonly IReadOnlyDictionary<CodePointPair, uint> _compositionTable;
        private readonly IReadOnlyDictionary<uint, int> _quickCheckTable;

        public Normalizer(UnicodeDataRecord[] unicodeData, NormalizationProps normalizationProps)
        {
            // CCCテーブル (共通)
            _canonicalCombiningClassTable = unicodeData.Where(x => x.CanonicalCombiningClass != 0)
                .ToDictionary(x => x.CodePoint, x => x.CanonicalCombiningClass);

            // 分解テーブル (Decomposer)
            _decompositionTable = CreateDecompositionTable(unicodeData);

            // 合成テーブル (Composer, OptimizedComposer)
            _compositionTable = CreateCompositionTable(unicodeData, normalizationProps);

            // クイックチェックテーブル (OptimizedComposer)
            _quickCheckTable = CreateQuickCheckTable(unicodeData, normalizationProps);
        }

        private int GetCanonicalCombiningClass(uint c)
        {
            int ccc;
            _canonicalCombiningClassTable.TryGetValue(c, out ccc);
            return ccc;
        }
    }
}
