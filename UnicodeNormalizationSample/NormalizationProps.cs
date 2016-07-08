using System;
using System.Collections.Generic;
using System.IO;

namespace UnicodeNormalizationSample
{
    public class NormalizationProps
    {
        public IReadOnlyList<uint> FullCompositionExclusion { get; }
        public IReadOnlyDictionary<uint, QuickCheckValue> NfcQC { get; }
        public IReadOnlyDictionary<uint, QuickCheckValue> NfkcQC { get; set; }

        public NormalizationProps(IReadOnlyList<uint> fullCompositionExclusion, IReadOnlyDictionary<uint, QuickCheckValue> nfcQC, IReadOnlyDictionary<uint, QuickCheckValue> nfkcQC)
        {
            this.FullCompositionExclusion = fullCompositionExclusion;
            this.NfcQC = nfcQC;
            this.NfkcQC = nfkcQC;
        }

        public static NormalizationProps Load(string fileName)
        {
            var fullCompositionExclusion = new List<uint>();
            var nfcQC = new Dictionary<uint, QuickCheckValue>();
            var nfkcQC = new Dictionary<uint, QuickCheckValue>();

            using (var reader = new StreamReader(fileName))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Length == 0) continue;
                    var commentStart = line.IndexOf('#');
                    if (commentStart == 0) continue;

                    var record = line.Substring(0, commentStart).Split(';');
                    switch (record[1].Trim())
                    {
                        case "Full_Composition_Exclusion":
                            fullCompositionExclusion.AddRange(Utils.ParseCodePointRange(record[0]));
                            break;
                        case "NFC_QC":
                            {
                                var qc = ParseQuickCheckValue(record[2]);
                                foreach (var cp in Utils.ParseCodePointRange(record[0]))
                                    nfcQC.Add(cp, qc);
                                break;
                            }
                        case "NFKC_QC":
                            {
                                var qc = ParseQuickCheckValue(record[2]);
                                foreach (var cp in Utils.ParseCodePointRange(record[0]))
                                    nfkcQC.Add(cp, qc);
                                break;
                            }
                    }
                }
            }

            return new NormalizationProps(fullCompositionExclusion, nfcQC, nfkcQC);
        }

        private static QuickCheckValue ParseQuickCheckValue(string s)
        {
            switch (s.Trim())
            {
                case "N": return QuickCheckValue.No;
                case "M": return QuickCheckValue.Maybe;
                default: throw new ArgumentException();
            }
        }
    }
}
