using System;

namespace UnicodeNormalizationSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var norm = new Normalizer(
                UnicodeData.Load("UnicodeData.txt"),
                NormalizationProps.Load("DerivedNormalizationProps.txt")
            );
            var tests = NormalizationTest.Load("NormalizationTest.txt");

            TestNFD(norm, tests);
            TestNFKD(norm, tests);
            TestNFC(norm, tests);
            TestNFKC(norm, tests);
            TestOptimizedNFC(norm, tests);
            TestOptimizedNFKC(norm, tests);
        }

        static void TestNFD(Normalizer norm, NormalizationTestRecord[] tests)
        {
            Func<uint[], uint[], bool> test = (input, expected) => norm.Decompose(input, false).ArrEq(expected);
            var passed = 0;
            foreach (var testCase in tests)
            {
                // c3 == toNFD(c1) == toNFD(c2) == toNFD(c3)
                // c5 == toNFD(c4) == toNFD(c5)
                if (test(testCase.C1, testCase.C3) && test(testCase.C2, testCase.C3) && test(testCase.C3, testCase.C3)
                    && test(testCase.C4, testCase.C5) && test(testCase.C5, testCase.C5))
                {
                    passed++;
                }
            }
            Console.WriteLine("NFD: {0} / {1}", passed, tests.Length);
        }

        static void TestNFKD(Normalizer norm, NormalizationTestRecord[] tests)
        {
            Func<uint[], uint[], bool> test = (input, exptected) => norm.Decompose(input, true).ArrEq(exptected);
            var passed = 0;
            foreach (var testCase in tests)
            {
                // c5 == toNFKD(c1) == toNFKD(c2) == toNFKD(c3) == toNFKD(c4) == toNFKD(c5)
                if (test(testCase.C1, testCase.C5) && test(testCase.C2, testCase.C5) && test(testCase.C3, testCase.C5)
                    && test(testCase.C4, testCase.C5) && test(testCase.C5, testCase.C5))
                {
                    passed++;
                }
            }
            Console.WriteLine("NFKD: {0} / {1}", passed, tests.Length);
        }

        static void TestNFC(Normalizer norm, NormalizationTestRecord[] tests)
        {
            Func<uint[], uint[], bool> test = (input, expected) => norm.Compose(input, false).ArrEq(expected);
            var passed = 0;
            foreach (var testCase in tests)
            {
                // c2 == toNFC(c1) == toNFC(c2) == toNFC(c3)
                // c4 == toNFC(c4) == toNFC(c5)
                if (test(testCase.C1, testCase.C2) && test(testCase.C2, testCase.C2) && test(testCase.C3, testCase.C2)
                    && test(testCase.C4, testCase.C4) && test(testCase.C5, testCase.C4))
                {
                    passed++;
                }
            }
            Console.WriteLine("NFC: {0} / {1}", passed, tests.Length);
        }

        static void TestNFKC(Normalizer norm, NormalizationTestRecord[] tests)
        {
            Func<uint[], uint[], bool> test = (input, expected) => norm.Compose(input, true).ArrEq(expected);
            var passed = 0;
            foreach (var testCase in tests)
            {
                // c4 == toNFKC(c1) == toNFKC(c2) == toNFKC(c3) == toNFKC(c4) == toNFKC(c5)
                if (test(testCase.C1, testCase.C4) && test(testCase.C2, testCase.C4) && test(testCase.C3, testCase.C4)
                    && test(testCase.C4, testCase.C4) && test(testCase.C5, testCase.C4))
                {
                    passed++;
                }
            }
            Console.WriteLine("NFKC: {0} / {1}", passed, tests.Length);
        }

        static void TestOptimizedNFC(Normalizer norm, NormalizationTestRecord[] tests)
        {
            Func<uint[], uint[], bool> test = (input, expected) => norm.ComposeOptimized(input, false).ArrEq(expected);
            var passed = 0;
            foreach (var testCase in tests)
            {
                // c2 == toNFC(c1) == toNFC(c2) == toNFC(c3)
                // c4 == toNFC(c4) == toNFC(c5)
                if (test(testCase.C1, testCase.C2) && test(testCase.C2, testCase.C2) && test(testCase.C3, testCase.C2)
                    && test(testCase.C4, testCase.C4) && test(testCase.C5, testCase.C4))
                {
                    passed++;
                }
            }
            Console.WriteLine("Optimized NFC: {0} / {1}", passed, tests.Length);
        }

        static void TestOptimizedNFKC(Normalizer norm, NormalizationTestRecord[] tests)
        {
            Func<uint[], uint[], bool> test = (input, expected) => norm.ComposeOptimized(input, true).ArrEq(expected);
            var passed = 0;
            foreach (var testCase in tests)
            {
                // c4 == toNFKC(c1) == toNFKC(c2) == toNFKC(c3) == toNFKC(c4) == toNFKC(c5)
                if (test(testCase.C1, testCase.C4) && test(testCase.C2, testCase.C4) && test(testCase.C3, testCase.C4)
                    && test(testCase.C4, testCase.C4) && test(testCase.C5, testCase.C4))
                {
                    passed++;
                }
            }
            Console.WriteLine("Optimized NFKC: {0} / {1}", passed, tests.Length);
        }
    }
}
