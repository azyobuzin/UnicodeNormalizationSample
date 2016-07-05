using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicSample
{
    // 合成・分解共通の処理は Normalizer.cs へ
    partial class Normalizer
    {
        /// <summary>正規合成を行います。</summary>
        /// <param name="input">UTF-32でエンコードされた文字列。</param>
        /// <param name="compatibility">互換分解するかどうか。</param>
        public uint[] Compose(uint[] input, bool compatibility)
        {
            input = Decompose(input, compatibility);

            if (input.Length <= 1)
                return input;


        }
    }
}
