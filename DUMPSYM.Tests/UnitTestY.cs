using System.Security.Cryptography;
using System.Text;

// ReSharper disable StringLiteralTypo

namespace DUMPSYM.Tests;

[TestClass]
public sealed class UnitTestY : UnitTestBase
{
    [TestMethod]
    public void TestMethodY()
    {
        var options = new HeaderGeneratorOptions
        {
            RemoveTypedefs = // TODO keep bool
            [
                "BBOOL",
                "BOOL",
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
                "UWORD"
            ],
            UseSdkUnsignedTypedefs = true
        };

        using var generator = new HeaderGenerator(Sample.Default, options);

        var contents = generator.Generate();

        const string path = @"C:\Files\GitHub\DUMPSYM\MAIN.SYM.H";

        File.WriteAllText(path, contents);

        var hash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(contents)));

        Assert.AreEqual("fce5510ea41c60461cda1e8be2d194afb613f551dc02ec691aa8e842adc02ae7", hash, true);
    }
}