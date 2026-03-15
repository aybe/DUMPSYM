// TODO cleanup/DRY

using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using DUMPSYM.Symbols;

namespace DUMPSYM.Generators;

public static class IdaGeneratorUtility
{
    public static string GenerateHeader(SymbolFile file)
    {
        var idaGenerator = GetSymbolGenerator(file);

        using var idaHeaderGenerator = new IdaHeaderGenerator(idaGenerator);

        var generate = idaHeaderGenerator.Generate();

        return generate;
    }

    public static void GenerateScripts(SymbolFile symbolFile, string targetDirectory)
    {
        File.WriteAllText(Path.Combine(targetDirectory, "dumpsym_output.txt"), symbolFile.ToString());

        var generator = GetSymbolGenerator(symbolFile);

        var scriptGenerator = new IdaScriptGenerator(generator);

        var output = scriptGenerator.Generate(symbolFile);

        var variables = symbolFile.Symbols.Where(s => s.IsVariable);

        foreach (var variable in variables)
        {
            if (output.Functions.Any(s => s.Header.Address == variable.Header.Address))
            {
                continue;
            }

            Console.WriteLine(variable);
        }

        var names = GetSymbolNamesScript(symbolFile);

        File.WriteAllText(Path.Combine(targetDirectory, "dumpsym_names.py"), names);

        var prototypes = output.GetFunctionsAsPythonList();

        File.WriteAllText(Path.Combine(targetDirectory, "dumpsym_function_prototypes.py"), prototypes);

        var functions = output.GetFunctionsAsDebugString();

        File.WriteAllText(Path.Combine(targetDirectory, "dumpsym_function_prototypes.txt"), functions);
    }

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public static IdaGenerator GetSymbolGenerator(SymbolFile file)
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

    public static string? GetSymbolNamesScript(SymbolFile file)
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