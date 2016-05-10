using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

class NormalizationTestRecord
{
    public uint[] C1 { get; }
    public uint[] C2 { get; }
    public uint[] C3 { get; }
    public uint[] C4 { get; }
    public uint[] C5 { get; }

    public NormalizationTestRecord(uint[] c1, uint[] c2, uint[] c3, uint[] c4, uint[] c5)
    {
        this.C1 = c1;
        this.C2 = c2;
        this.C3 = c3;
        this.C4 = c4;
        this.C5 = c5;
    }
}

static class NormalizationTest
{
    public static NormalizationTestRecord[] Load(string fileName)
    {
        var result = new List<NormalizationTestRecord>();
        using (var reader = new StreamReader(fileName))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Length == 0 || line[0] == '#' || line[0] == '@') continue;
                var record = line.Split(';').Take(5)
                    .Select(s => s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(Utils.ParseCodePoint)
                        .ToArray()
                    )
                    .ToArray();
                result.Add(new NormalizationTestRecord(record[0], record[1], record[2], record[3], record[4]));
            }
        }
        return result.ToArray();
    }
}
