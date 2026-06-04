namespace DUMPSYM.Symbols;

public interface ISymbolDefinition : ISymbol
{
    SymbolStorageClass Class { get; }

    SymbolType Type { get; }

    uint Size { get; }

    string Name { get; }
}