#define LOG

using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DUMPSYM;

public sealed class SymbolRegistry
{
    private static readonly Regex RegexFakeName = new(@"\.(\d+fake)", RegexOptions.Compiled);

    public required List<ISymbol> Externals { get; init; }

    public required List<List<ISymbol>> Files { get; init; }

    public required List<List<ISymbol>> Functions { get; init; }

    public required List<ISymbol> Names { get; init; }

    public required List<ISymbol> Statics { get; init; }

    public required List<List<ISymbol>> Structs { get; init; }

    public required List<ISymbol> Typedefs { get; init; }

    public required List<List<ISymbol>> Unions { get; init; }

    public List<Code> CreateLists()
    {
        var codes = new List<Code>();

        codes.AddRange(Externals.Select(s => new Code(s)));
        codes.AddRange(Files.Select(s => new Code(s, 0)));
        codes.AddRange(Functions.Select(s => new Code(s, 0)));
        codes.AddRange(Names.Select(s => new Code(s)));
        codes.AddRange(Statics.Select(s => new Code(s)));
        codes.AddRange(Structs.Select(s => new Code(s, 0)));
        codes.AddRange(Typedefs.Select(s => new Code(s)));
        codes.AddRange(Unions.Select(s => new Code(s, 0)));

        return codes;
    }

    public static string GetSafeName(string name)
    {
        return RegexFakeName.Match(name) is { Success: true } m ? $"_{m.Groups[1].Value}" : name;
    }

    public static bool HasFakeName(string name)
    {
        return RegexFakeName.IsMatch(name);
    }

    private static bool HasRealName(string name)
    {
        return HasFakeName(name) is false;
    }

    public void Parse()
    {
        // TODO https://www.beneaththewaves.net/Software/This_Dust_Remembers_What_It_Once_Was.html

        // TODO struct _98fake, member .97fake srm // UNION

        Log("// typedefs (real)");

        foreach (var symbol in Typedefs.Cast<ISymbolDefinition>().Where(s => s is not ISymbolDefinition2))
        {
            var s = GetTypedefString(symbol);

            Log(s);
        }

        Log();

        Log("// TODO fake structs");

        foreach (var symbols in Structs.Where(s => s[0] is ISymbolDefinition def && HasFakeName(def.Name)))
        {
            var s = GetTypeString(symbols);

            Log(s);
        }

        Log();

        Log("// typedefs (fake)");

        foreach (var symbol in Typedefs.Cast<ISymbolDefinition>().OfType<ISymbolDefinition2>())
        {
            var s = GetTypedefString(symbol);

            Log(s);
        }

        Console.WriteLine("// TODO real structs"); // TODO log

        foreach (var symbols in Structs.Where(s => s[0] is ISymbolDefinition def && HasRealName(def.Name)))
        {
            var s = GetTypeString(symbols);

            Console.WriteLine(s); // TODO log
        }
    }

    private string GetTypeString(List<ISymbol> symbols)
    {
        using var writer = new IndentedTextWriter(new StringWriter());

        var header = (ISymbolDefinition)symbols[0];

        var structName = GetSafeName(header.Name);

        if (structName != "_6fake")
        {
            //return null;
        }

        writer.WriteLine($"struct {structName}");
        writer.WriteLine("{");
        writer.Indent++;

        foreach (var symbol in symbols[1..^1].Cast<ISymbolDefinition>())
        {
            var type = Typedefs.Cast<ISymbolDefinition>().FirstOrDefault(s => s.Type.Kind == symbol.Type.Kind);

            var typeName = type?.Name ?? symbol.Type.Kind.ToString().ToLowerInvariant(); // TODO INT has no typedef in .SYM file

            if (symbol is ISymbolDefinition2 def)
            {
                if (def.Tag != string.Empty)
                {
                    var tag = Typedefs.OfType<ISymbolDefinition2>().FirstOrDefault(s => s.Tag == def.Tag); // TODO figure out why this may fail

                    writer.Write($"{tag?.Name ?? def.Tag} {symbol.Name}");
                }
                else
                {
                    writer.Write($"{typeName} {symbol.Name}");
                }

                for (var i = 0; i < def.Type.Modifiers.Count(s => s == SymbolTypeModifier.ARY); i++)
                {
                    writer.Write($"[{def.Dimensions[i]}]");
                }

                writer.Write(";");

                writer.Write($" // {symbol}"); // TODO

                writer.WriteLine();
            }
            else
            {
                writer.WriteLine($"{typeName} {symbol.Name}; // {symbol}");
            }
        }

        writer.Indent--;
        writer.WriteLine("};");

        return writer.InnerWriter.ToString()!;
    }

    private static string GetTypedefString(ISymbolDefinition symbol)
    {
        using var writer = new IndentedTextWriter(new StringWriter());

        var type = TypedefUtility.GetKindString(symbol.Type.Kind);

        var pointers = new string('*', symbol.Type.Modifiers.Count(s => s == SymbolTypeModifier.PTR));

        if (symbol.Type.Modifiers.Contains(SymbolTypeModifier.FCN))
        {
            writer.Write($"typedef {type} ({pointers}{symbol.Name})();");
        }
        else
        {
            if (symbol is ISymbolDefinition2 def)
            {
                var tag = def.Tag;

                if (tag.StartsWith('.'))
                {
                    tag = $"_{tag[1..]}";
                }

                writer.Write($"typedef {type} {tag}{pointers} {symbol.Name};");
            }
            else
            {
                writer.Write($"typedef {type}{pointers} {symbol.Name};");
            }
        }

        return $"{writer.InnerWriter} // {symbol}";
    }

    private void ParseFakeStruct(List<ISymbol> symbols)
    {
        var header = (ISymbolDefinition)symbols[0];

        Assert.AreEqual(SymbolStorageClass.STRTAG, header.Class);

        Assert.AreEqual(SymbolTypeKind.STRUCT, header.Type.Kind);

        var members = symbols[1..^1];

        using var writer = new IndentedTextWriter(new StringWriter());

        var structName = RegexFakeName.IsMatch(header.Name) ? $"_{header.Name[1..]}" : header.Name;

        writer.WriteLine($"struct {structName}");

        writer.WriteLine("{");

        writer.Indent++;

        foreach (var member in members.Cast<ISymbolDefinition>())
        {
            if (member is ISymbolDefinition2)
            {
                writer.WriteLine($"// TODO {member}"); // TODO
            }
            else
            {
                var memberType = Typedefs.Cast<ISymbolDefinition>().FirstOrDefault(s => s.Type == member.Type);

                var memberTypeName =
                    memberType?.Name ?? member.Type.Kind.ToString().ToLowerInvariant(); // TODO improve crude hack, cause = no typedef for INT 

                writer.Write($"{memberTypeName} {member.Name}");

                if (member.Class is SymbolStorageClass.FIELD)
                {
                    writer.Write($" : {member.Size}");
                }
                else
                {
                    Assert.AreEqual(SymbolStorageClass.MOS, member.Class);
                }

                writer.WriteLine(";");
            }
        }

        writer.Indent--;

        writer.WriteLine("};");

        var str = writer.InnerWriter.ToString();

        foreach (var member in members)
        {
            Log(member);
        }

        Console.WriteLine(str);
    }

    private void ParseTypedefs()
    {
        var basics = ParseTypedefsBasic();

        foreach (var source in basics)
        {
            Log(source);
        }

        Assert.IsTrue(Typedefs.All(s => s is ISymbolDefinition2));

        var pointers = ParseTypedefsPointer();

        foreach (var source in pointers)
        {
            Log(source);
        }

        Log($"Remaining typedefs: {Typedefs.Count}");

        foreach (var symbol in Typedefs)
        {
            Log(symbol);
        }
    }

    private Source[] ParseTypedefsBasic()
    {
        var definitions = Typedefs
            .Cast<ISymbolDefinition>()
            .Where(s => s is not ISymbolDefinition2)
            .ToArray();

        var sources = definitions.Select(s => new Source([s], TypedefUtility.ParseSimple(s))).ToArray();

        Log($"Parsed {definitions.Length} basic typedefs out of {Typedefs.Count}");

        Typedefs.RemoveAll(s => definitions.Contains(s));

        return sources;
    }

    private Source[] ParseTypedefsPointer()
    {
        var definitions = Typedefs
            .Cast<ISymbolDefinition2>()
            .Where(s => s.Type.Kind == SymbolTypeKind.STRUCT && s.Type.Modifiers.Contains(SymbolTypeModifier.PTR))
            .ToArray();

        var sources = definitions.Select(s => new Source([s], TypedefUtility.ParseComplex(s))).ToArray();

        Log($"Parsed {definitions.Length} pointer typedefs out of {Typedefs.Count}");

        Typedefs.RemoveAll(s => definitions.Contains(s));

        return sources;
    }

    [Conditional("LOG")]
    private static void Log(object? value = null)
    {
        Console.WriteLine(value?.ToString());
    }
}

public class Source
{
    public Source(List<ISymbol> symbols, string text)
    {
        Symbols = symbols;
        Text = text;
    }

    public List<ISymbol> Symbols { get; init; }

    public string Text { get; init; }

    public override string ToString()
    {
        return $"{Text}";
    }
}