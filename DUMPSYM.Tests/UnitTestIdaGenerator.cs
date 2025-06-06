using System.Security.Cryptography;
using System.Text;
using DUMPSYM.Generators;

// ReSharper disable StringLiteralTypo

namespace DUMPSYM.Tests;

[TestClass]
public sealed class UnitTestIdaGenerator : UnitTestBase
{
    private static IdaGenerator GetGenerator()
    {
        var file = Sample.Default;

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

        var generator = new IdaGenerator(file.Symbols.ToList(), options);

        return generator;
    }

    [TestMethod]
    public void TestHeaderGenerator()
    {
        var generator = GetGenerator();

        using var headerGenerator = new IdaHeaderGenerator(generator);

        var generate = headerGenerator.Generate();

        WriteLine(generate);

        File.WriteAllText(@"C:\Files\GitHub\DUMPSYM\MAIN.SYM.H", generate);

        Validate(generate, "a7cdae4d1fe81d23953e77bce5614ca4cde7c03128af8a437087d83c0cc4d523");
    }

    [TestMethod]
    public void TestScriptGenerator()
    {
        var generator = GetGenerator();

        var scriptGenerator = new IdaScriptGenerator(generator);

        var output = scriptGenerator.Generate(Sample.Default);

        WriteFunctionPrototypes(output);

        var functions = output.GetFunctionsAsDebugString();

        WriteLine(functions);

        File.WriteAllText(@"C:\Files\GitHub\DUMPSYM\MAIN.SYM.OUT", functions);

        Validate(functions, "f83951a7085e577083e73b5b14eb9477c9e8b7fb55990a773c29c6304715ab7e");
    }

    private static void Validate(string text, string sha256)
    {
        var hash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text)));

        Assert.AreEqual(sha256, hash, true);
    }

    private void WriteFunctionPrototypes(IdaScriptGeneratorOutput output)
    {
        var prototypes = output.GetFunctionsAsPythonList();

        const string directory = @"C:\Files\GitHub\DUMPSYM\IDA";

        Directory.CreateDirectory(directory);

        var path = Path.Combine(directory, "dumpsym_function_prototypes.py");

        File.WriteAllText(path, prototypes);
    }
}