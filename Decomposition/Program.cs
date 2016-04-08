using System;

namespace Decomposition
{
    class Program
    {
        static void Main(string[] args)
        {
            var decomposer = new Decomposer(UnicodeData.Load("UnicodeData.txt"));
            var tests = NormalizationTest.Load("NormalizationTest.txt");

            TestNFD(decomposer, tests);
            TestNFKD(decomposer, tests);

            Console.ReadLine();
        }

        static void TestNFD(Decomposer decomposer, NormalizationTestRecord[] tests)
        {
            Func<uint[], uint[]> decomp = input => decomposer.Decompose(input, false);
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

        static void TestNFKD(Decomposer decomposer, NormalizationTestRecord[] tests)
        {
            Func<uint[], uint[]> decomp = input => decomposer.Decompose(input, true);
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
