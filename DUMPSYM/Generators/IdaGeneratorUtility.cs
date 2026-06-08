// TODO cleanup/DRY

using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
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

        File.WriteAllText(Path.Combine(targetDirectory, "dumpsym.py"), GetEmbeddedResourceAsString("DUMPSYM.dumpsym.py"));

        var generator = GetSymbolGenerator(symbolFile);

        var scriptGenerator = new IdaScriptGenerator(generator);

        var output = scriptGenerator.Generate(symbolFile);

        var names = GetSymbolNamesScript(symbolFile);

        File.WriteAllText(Path.Combine(targetDirectory, "dumpsym_names.py"), names);

        var prototypes = output.GetFunctionsAsPythonList();

        File.WriteAllText(Path.Combine(targetDirectory, "dumpsym_function_prototypes.py"), prototypes);

        var functions = output.GetFunctionsAsDebugString();

        File.WriteAllText(Path.Combine(targetDirectory, "dumpsym_function_prototypes.txt"), functions);

        var registers = GetScriptForFunctionsRegisters(symbolFile);

        File.WriteAllText(Path.Combine(targetDirectory, "dumpsym_function_registers.py"), registers);

        Console.WriteLine(registers); // TODO remove
    }

    private static string GetEmbeddedResourceAsString(string name)
    {
        var assembly = Assembly.GetExecutingAssembly();

        using var stream = assembly.GetManifestResourceStream(name)!;

        using var reader = new StreamReader(stream, Encoding.UTF8);

        var text = reader.ReadToEnd();

        return text;
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

    #region Registers

    private static string[] MipsRegisterNames { get; } =
    [
        "$zero",
        "$at",
        "$v0",
        "$v1",
        "$a0",
        "$a1",
        "$a2",
        "$a3",
        "$t0",
        "$t1",
        "$t2",
        "$t3",
        "$t4",
        "$t5",
        "$t6",
        "$t7",
        "$s0",
        "$s1",
        "$s2",
        "$s3",
        "$s4",
        "$s5",
        "$s6",
        "$s7",
        "$t8",
        "$t9",
        "$k0",
        "$k1",
        "$gp",
        "$sp",
        "$fp",
        "$ra",
        "$f0",
        "$f1",
        "$f2",
        "$f3",
        "$f4",
        "$f5",
        "$f6",
        "$f7",
        "$f8",
        "$f9",
        "$f10",
        "$f11",
        "$f12",
        "$f13",
        "$f14",
        "$f15",
        "$f16",
        "$f17",
        "$f18",
        "$f19",
        "$f20",
        "$f21",
        "$f22",
        "$f23",
        "$f24",
        "$f25",
        "$f26",
        "$f27",
        "$f28",
        "$f29",
        "$f30",
        "$f31",
        "pc",
        "cs",
        "ds",
        "mips16",
        "gp",
    ];

    private static string GetScriptForFunctionsRegisters(SymbolFile symbolFile)
        // TODO apply variable type
        // TODO fp, fsize, retreg, mask, maskoffs, line, file
        // TODO handle reuse of same regs for different variables
        // TODO cleanup
    {
        var functions = GetFunctionsUsingRegisters(symbolFile.Symbols);

        using var writer = new IndentedTextWriter(new StringWriter());

        foreach (var symbols in functions)
        {
            WriteFunctionRegisters(symbols, writer);

            writer.WriteLine();
        }

        return writer.InnerWriter.ToString()!;
    }

    private static void WriteFunctionRegisters(Symbol[] function, IndentedTextWriter writer)
    {
        var regs = function.Where(s => s.IsRegister).DistinctBy(s => s.Record).ToLookup(s => s.Header.Address);
        var func = function[0];
        var addr = $"0x{func.Header.Address:X8}";
        var name = ((ISymbolFunction)func.Record).Name;

        if (regs.Any(s => s.Count() > 1))
        {
            writer.WriteLine($"# ERROR_REGS_USAGE_FUNC: [{addr}, {name}]");

            foreach (var symbols in regs.Where(s => s.Count() > 1))
            {
                writer.WriteLine(
                    $"# ERROR_REGS_USAGE_VARS: [{addr}, {name}], " +
                    $"[{MipsRegisterNames[symbols.Key]}, " +
                    $"[{string.Join(", ", symbols.Select(s => ((ISymbolDefinition)s.Record).Name))}]]");
            }
        }
        else
        {
            writer.WriteLine("dumpsym_rename_func_regs(");
            writer.Indent++;
            writer.WriteLine($"{addr}, # {name}");
            writer.WriteLine("[");
            writer.Indent++;

            foreach (var list in regs)
            {
                foreach (var (header, record) in list)
                {
                    writer.WriteLine(
                        $"(0x{header.Address:X2}, \"{((ISymbolDefinition)record).Name}\"), " +
                        $"# {MipsRegisterNames[header.Address]}");
                }
            }

            writer.Indent--;
            writer.WriteLine("],");
            writer.Indent--;
            writer.WriteLine(")");
        }
    }

    private static List<Symbol[]> GetFunctionsUsingRegisters(List<Symbol> symbols)
    {
        var functions = new List<Symbol[]>();

        foreach (var (index, symbol) in symbols.Index())
        {
            if (!symbol.IsFunction)
            {
                continue;
            }

            var function = symbols[index..symbols.FindIndex(index, s => s.IsFunctionEnd)];

            var array = function.Where(s => s.IsFunction || s.IsRegister).ToArray();

            if (array.Length > 1)
            {
                functions.Add(array);
            }
        }

        return functions;
    }

    #endregion
}