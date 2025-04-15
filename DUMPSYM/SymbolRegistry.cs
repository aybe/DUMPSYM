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
    }
}