using System.CodeDom.Compiler;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using DUMPSYM.Generators;
using DUMPSYM.Symbols;
using JetBrains.Annotations;

// ReSharper disable StringLiteralTypo

namespace DUMPSYM.Tests;

[TestClass]
[UsedImplicitly]
public sealed class UnitTestIdaGenerator : UnitTestBase
{
    private static string OutputDirectory { get; } = Directory.CreateDirectory(Path.Combine(Solution.Directory, "Output")).FullName;

    private static string IdaOutputDirectory { get; } = Directory.CreateDirectory(Path.Combine(OutputDirectory, "IDA")).FullName;

    private static SymbolFile GetSampleFile(string path = @"C:\GitHub\DUMPSYM\MAIN.SYM")
    {
        using var stream = File.OpenRead(path);

        var file = SymbolFile.Dump(stream);

        return file;
    }

    private static IdaGenerator GetGenerator(string path = @"C:\GitHub\DUMPSYM\MAIN.SYM")
    {
        var file = GetSampleFile(path);

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
    [UsedImplicitly]
    [DynamicData(nameof(TestHeaderGeneratorData), DynamicDataDisplayName = nameof(TestHeaderGeneratorName))]
    public void TestHeaderGenerator(string sourceFile, string targetDirectory)
    {
        if (!File.Exists(sourceFile))
        {
            throw new FileNotFoundException(null, sourceFile);
        }

        var generator = GetGenerator(sourceFile);

        using var headerGenerator = new IdaHeaderGenerator(generator);

        var generate = headerGenerator.Generate();

        WriteLine(generate);

        Directory.CreateDirectory(targetDirectory);

        var path = Path.Combine(targetDirectory, Path.ChangeExtension(Path.GetFileNameWithoutExtension(sourceFile), ".H"));

        File.WriteAllText(path, generate);

        switch (Path.GetFileName(sourceFile)) // TODO others
        {
            case "MAIN.SYM":
                Validate(generate, "a7cdae4d1fe81d23953e77bce5614ca4cde7c03128af8a437087d83c0cc4d523");
                break;
        }
    }

    public static string TestHeaderGeneratorName(MethodInfo methodInfo, object[] data)
    {
        return $"{methodInfo.Name}(\"{Path.GetFileName(data[0] as string)}\")";
    }

    public static IEnumerable<object[]> TestHeaderGeneratorData() // TODO use JSON file
    {
        return
        [
            [
                @"C:\GitHub\DUMPSYM\Tests\Source\Destruction Derby (Japan)\DEMOLISH.SYM",
                @"C:\GitHub\DUMPSYM\Tests\Target\Destruction Derby (Japan)",
            ],
            [
                @"C:\GitHub\DUMPSYM\Tests\Source\Hi-Octane (Europe) (En,Fr,De,Es)\MAIN.SYM",
                @"C:\GitHub\DUMPSYM\Tests\Target\Hi-Octane (Europe) (En,Fr,De,Es)",
            ],
            [
                @"C:\GitHub\DUMPSYM\Tests\Source\Wipeout XL (USA) (Beta)\NTSC.SYM",
                @"C:\GitHub\DUMPSYM\Tests\Target\Wipeout XL (USA) (Beta)",
            ],
        ];
    }

    [TestMethod]
    public void TestScriptGenerator()
    {
        var generator = GetGenerator();

        var scriptGenerator = new IdaScriptGenerator(generator);

        var file = GetSampleFile();

        var output = scriptGenerator.Generate(file);

        var variables = file.Symbols.Where(s => s.IsVariable);

        foreach (var variable in variables)
        {
            if (output.Functions.Any(s => s.Header.Address == variable.Header.Address))
            {
                continue;
            }

            Console.WriteLine(variable);
        }

        GenerateScriptForNames(file);

        WriteFunctionPrototypes(output);

        var functions = output.GetFunctionsAsDebugString();

        WriteLine(functions);

        File.WriteAllText(Path.Combine(OutputDirectory, "MAIN.SYM.OUT"), functions);

        Validate(functions, "f83951a7085e577083e73b5b14eb9477c9e8b7fb55990a773c29c6304715ab7e");
    }

    private static void GenerateScriptForNames(SymbolFile file)
    {
        using var writer = new IndentedTextWriter(new StringWriter());

        var names = file.Symbols.Where(s => s.IsVariable).ToArray();

        writer.WriteLine($"# {names.Length} names");

        writer.WriteLine("dumpsym_names = [");

        writer.Indent++;

        foreach (var symbol in names)
        {
            writer.WriteLine("""(0x{0:X8}, "{1}"),""", symbol.Header.Address, ((ISymbolVariable)symbol.Record).Name);
        }

        writer.Indent--;

        writer.WriteLine("]");

        File.WriteAllText(Path.Combine(IdaOutputDirectory, "dumpsym_names.py"), writer.InnerWriter.ToString());
    }

    private static void Validate(string text, string sha256)
    {
        var hash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text)));

        Assert.AreEqual(sha256, hash, true);
    }

    private void WriteFunctionPrototypes(IdaScriptGeneratorOutput output)
    {
        var prototypes = output.GetFunctionsAsPythonList();

        var path = Path.Combine(IdaOutputDirectory, "dumpsym_function_prototypes.py");

        File.WriteAllText(path, prototypes);
    }
}