using System;

namespace BasicSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var norm = new Normalizer(UnicodeData.Load("UnicodeData.txt"));
            var tests = NormalizationTest.Load("NormalizationTest.txt");

            TestNFD(norm, tests);
            TestNFKD(norm, tests);

            Console.ReadLine();
        }

        static void TestNFD(Normalizer norm, NormalizationTestRecord[] tests)
        {
            Func<uint[], uint[]> decomp = input => norm.Decompose(input, false);
            var passed = 0;
            foreach (var test in tests)
            {
                // c3 == toNFD(c1) == toNFD(c2) == toNFD(c3)
                // c5 == toNFD(c4) == toNFD(c5)
                if (decomp(test.C1).ArrEq(test.C3) && decomp(test.C2).ArrEq(test.C3) && decomp(test.C3).ArrEq(test.C3)
                    && decomp(test.C4).ArrEq(test.C5) && decomp(test.C5).ArrEq(test.C5))
                {
                    passed++;
                }
            }
            Console.WriteLine("NFD: {0} / {1}", passed, tests.Length);
        }

        static void TestNFKD(Normalizer norm, NormalizationTestRecord[] tests)
        {
            Func<uint[], uint[]> decomp = input => norm.Decompose(input, true);
            var passed = 0;
            foreach (var test in tests)
            {
                // c5 == toNFKD(c1) == toNFKD(c2) == toNFKD(c3) == toNFKD(c4) == toNFKD(c5)
                if (decomp(test.C1).ArrEq(test.C5) && decomp(test.C2).ArrEq(test.C5) && decomp(test.C3).ArrEq(test.C5)
                    && decomp(test.C4).ArrEq(test.C5) && decomp(test.C5).ArrEq(test.C5))
                {
                    passed++;
                }
            }
            Console.WriteLine("NFKD: {0} / {1}", passed, tests.Length);
        }
    }
}
