using System.Collections.Generic;
using System.IO;

public class NormalizationProps
{
    public IReadOnlyList<uint> FullCompositionExclusion { get; }

    public NormalizationProps(IReadOnlyList<uint> fullCompositionExclusion)
    {
        this.FullCompositionExclusion = fullCompositionExclusion;
    }

    public static NormalizationProps Load(string fileName)
    {
        var fullCompositionExclusion = new List<uint>();

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
                }
            }
        }

        return new NormalizationProps(fullCompositionExclusion);
    }
}
