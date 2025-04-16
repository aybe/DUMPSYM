#define LOG
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DUMPSYM;

public sealed class SymbolRegistry
{
    public required List<ISymbol> Externals { get; init; }

    public required List<List<ISymbol>> Files { get; init; }

    public required List<List<ISymbol>> Functions { get; init; }

    public required List<ISymbol> Names { get; init; }

    public required List<ISymbol> Statics { get; init; }

    public required List<List<ISymbol>> Structs { get; init; }

    public required List<ISymbol> Typedefs { get; init; }

    public required List<List<ISymbol>> Unions { get; init; }

    public void Parse()
    {
        ParseTypedefs();

        // TODO convert structs/unions using typedefs if needed
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
    private static void Log(object? value)
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