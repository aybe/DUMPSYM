using System.Security.Cryptography;
using System.Text;
using DUMPSYM.Generators;

// ReSharper disable StringLiteralTypo

namespace DUMPSYM.Tests;

[TestClass]
public sealed class UnitTestIdaGenerator : UnitTestBase
{
    [TestMethod]
    public void TestHeaderGenerator()
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

    [TestMethod]
    public void TestScriptGenerator()
    {
        var generator = new IdaScriptGenerator();

        var contents = generator.Generate(Sample.Default);

        WriteLine(contents);

        var hash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(contents)));

        Assert.AreEqual("0023f3ddce4faa080116cb2db0c8f1dd68db0beb1f14914bd7f80228eb2ccb26", hash, true);
    }
}