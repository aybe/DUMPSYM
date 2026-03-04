using System.CodeDom.Compiler;
using DUMPSYM.Generators;
using DUMPSYM.Symbols;
using JetBrains.Annotations;

// ReSharper disable StringLiteralTypo

namespace DUMPSYM.Tests;

[TestClass]
[UsedImplicitly]
public sealed class TestIdaGenerator : TestBase
{
    [TestMethod]
    [UsedImplicitly]
    [DynamicData(nameof(GetDynamicTestData), DynamicDataDisplayName = nameof(GetDynamicTestName), DynamicDataDisplayNameDeclaringType = typeof(TestBase))]
    public void TestGenerateHeader(string sourcePath, string targetPath)
    {
        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException(null, sourcePath);
        }

        var file = GetSymbolFile(sourcePath);

        var generator = GetSymbolGenerator(file);

        using var headerGenerator = new IdaHeaderGenerator(generator);

        var generate = headerGenerator.Generate();

        WriteLine(generate);

        Directory.CreateDirectory(targetPath);

        var path = Path.Combine(targetPath, Path.ChangeExtension(Path.GetFileNameWithoutExtension(sourcePath), ".H"));

        File.WriteAllText(path, generate);
    }

    [TestMethod]
    [UsedImplicitly]
    [DynamicData(nameof(GetDynamicTestData), DynamicDataDisplayName = nameof(GetDynamicTestName), DynamicDataDisplayNameDeclaringType = typeof(TestBase))]
    public void TestGenerateScript(string sourcePath, string targetPath)
    {
        var file = GetSymbolFile(sourcePath);

        File.WriteAllText(Path.Combine(targetPath, Path.ChangeExtension(Path.GetFileNameWithoutExtension(sourcePath), ".dumpsym.txt")), file.ToString());

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

        File.WriteAllText(Path.Combine(targetPath, "dumpsym_names.py"), names);

        var prototypes = output.GetFunctionsAsPythonList();

        File.WriteAllText(Path.Combine(targetPath, "dumpsym_function_prototypes.py"), prototypes);

        var functions = output.GetFunctionsAsDebugString();

        WriteLine(functions);

        var name = Path.GetFileName(sourcePath);

        File.WriteAllText(Path.Combine(targetPath, Path.ChangeExtension(name, ".functions.txt")), functions);
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
}