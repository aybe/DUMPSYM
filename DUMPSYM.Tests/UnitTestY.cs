using System.Security.Cryptography;
using System.Text;
using DUMPSYM.Generators;

// ReSharper disable StringLiteralTypo

namespace DUMPSYM.Tests;

[TestClass]
public sealed class UnitTestY : UnitTestBase
{
    [TestMethod]
    public void TestMethodY()
    {
        var options = new IdaHeaderGeneratorOptions
        {
            RemoveTypedefs =
            [
                "PSBYTE",
                "PSLONG",
                "PSWORD",
                "PUBYTE",
                "PULONG",
                "PUWORD",
                "SBYTE",
                "SLONG",
                "SWORD",
                "UBYTE",
                "ULONG",
                "UWORD",
            ],
        };

        var generator = new IdaGenerator(Sample.Default.Symbols.ToList(), options);

        using var headerGenerator = new IdaHeaderGenerator(generator);

        var contents = headerGenerator.Generate();

        const string path = @"C:\Files\GitHub\DUMPSYM\MAIN.SYM.H";

        File.WriteAllText(path, contents);

        var hash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(contents)));

        Assert.AreEqual("a7cdae4d1fe81d23953e77bce5614ca4cde7c03128af8a437087d83c0cc4d523", hash, true);
    }
}