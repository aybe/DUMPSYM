using System.CodeDom.Compiler;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using DUMPSYM.Generators;
using DUMPSYM.Symbols;
using JetBrains.Annotations;
using Newtonsoft.Json;

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
    public void TestHeaderGenerator(TestPair pair)
    {
        var (source, target) = pair;

        if (!File.Exists(source))
        {
            throw new FileNotFoundException(null, source);
        }

        var generator = GetGenerator(source);

        using var headerGenerator = new IdaHeaderGenerator(generator);

        var generate = headerGenerator.Generate();

        WriteLine(generate);

        Directory.CreateDirectory(target);

        var path = Path.Combine(target, Path.ChangeExtension(Path.GetFileNameWithoutExtension(source), ".H"));

        File.WriteAllText(path, generate);

        switch (Path.GetFileName(source)) // TODO others
        {
            case "MAIN.SYM":
                Validate(generate, "a7cdae4d1fe81d23953e77bce5614ca4cde7c03128af8a437087d83c0cc4d523");
                break;
        }
    }

    public static string TestHeaderGeneratorName(MethodInfo methodInfo, object[] data)
    {
        return $"{methodInfo.Name}(\"{Path.GetFileName(((TestPair)data[0]).Source)}\")";
    }

    public static IEnumerable<object[]> TestHeaderGeneratorData()
    {
        var path = Path.Combine(Solution.Directory, "TestData", "test-ida-header-generator.json");

        var text = File.ReadAllText(path);

        var data = JsonConvert.DeserializeObject<TestPair[]>(text)!;

        foreach (var pair in data)
        {
            yield return [pair];
        }
    }

    [TestMethod]
    [UsedImplicitly]
    [DynamicData(nameof(TestHeaderGeneratorData), DynamicDataDisplayName = nameof(TestHeaderGeneratorName))]
    public void TestScriptGenerator(TestPair pair)
    {
        var generator = GetGenerator(pair.Source);

        var scriptGenerator = new IdaScriptGenerator(generator);

        var file = GetSampleFile(pair.Source);

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

        var name = Path.GetFileName(pair.Source);

        File.WriteAllText(Path.Combine(pair.Target, Path.ChangeExtension(name, ".out")), functions);

        switch (name) // TODO others
        {
            case "MAIN.SYM":
                Validate(functions, "f83951a7085e577083e73b5b14eb9477c9e8b7fb55990a773c29c6304715ab7e");
                break;
        }
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

    [PublicAPI]
    public sealed record TestPair
    {
        public required string Source { get; set; }

        public required string Target { get; set; }

        public void Deconstruct(out string source, out string target)
        {
            source = Source;
            target = Target;
        }
    }
}