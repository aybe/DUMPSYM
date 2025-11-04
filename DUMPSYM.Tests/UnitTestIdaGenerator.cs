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
    [TestMethod]
    [UsedImplicitly]
    [DynamicData(nameof(GetTestData), DynamicDataDisplayName = nameof(GetTestName))]
    public void TestHeaderGenerator(TestPair pair)
    {
        var (source, target) = pair;

        if (!File.Exists(source))
        {
            throw new FileNotFoundException(null, source);
        }

        var file = GetSymbolFile(source);

        var generator = GetSymbolGenerator(file);

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

    [TestMethod]
    [UsedImplicitly]
    [DynamicData(nameof(GetTestData), DynamicDataDisplayName = nameof(GetTestName))]
    public void TestScriptGenerator(TestPair pair)
    {
        var file = GetSymbolFile(pair.Source);

        File.WriteAllText(Path.Combine(pair.Target, Path.ChangeExtension(Path.GetFileNameWithoutExtension(pair.Source), ".dumpsym.txt")), file.ToString());

        var generator = GetSymbolGenerator(file);

        var scriptGenerator = new IdaScriptGenerator(generator);

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

        var names = GetSymbolNamesScript(file);

        File.WriteAllText(Path.Combine(pair.Target, "dumpsym_names.py"), names);

        var prototypes = output.GetFunctionsAsPythonList();

        File.WriteAllText(Path.Combine(pair.Target, "dumpsym_function_prototypes.py"), prototypes);

        var functions = output.GetFunctionsAsDebugString();

        WriteLine(functions);

        var name = Path.GetFileName(pair.Source);

        File.WriteAllText(Path.Combine(pair.Target, Path.ChangeExtension(name, ".functions.txt")), functions);

        switch (name) // TODO others
        {
            case "MAIN.SYM":
                Validate(functions, "f83951a7085e577083e73b5b14eb9477c9e8b7fb55990a773c29c6304715ab7e");
                break;
        }
    }

    public static IEnumerable<object[]> GetTestData()
    {
        var path = Path.Combine(Solution.Directory, "Tests", "test-ida-generators.json");

        var text = File.ReadAllText(path);

        var data = JsonConvert.DeserializeObject<TestPair[]>(text)!;

        foreach (var pair in data)
        {
            yield return [pair];
        }
    }

    public static string GetTestName(MethodInfo methodInfo, object[] data)
    {
        return $"{methodInfo.Name}(\"{Path.GetFileName(((TestPair)data[0]).Source)}\")";
    }

    private static SymbolFile GetSymbolFile(string path)
    {
        using var stream = File.OpenRead(path);

        var file = SymbolFile.Dump(stream);

        return file;
    }

    private static IdaGenerator GetSymbolGenerator(SymbolFile file)
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

        var generator = new IdaGenerator(file.Symbols.ToList(), options);

        return generator;
    }

    private static string? GetSymbolNamesScript(SymbolFile file)
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

        writer.Flush();

        var contents = writer.InnerWriter.ToString();

        return contents;
    }

    private static void Validate(string text, string sha256)
    {
        var hash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text)));

        Assert.AreEqual(sha256, hash, true);
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