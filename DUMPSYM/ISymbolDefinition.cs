namespace DUMPSYM;

public interface ISymbolDefinition
{
    SymbolStorageClass Class { get; }

    SymbolType Type { get; }

    uint Size { get; }

    string Name { get; }
}