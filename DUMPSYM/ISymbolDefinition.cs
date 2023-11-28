namespace DUMPSYM;

public interface ISymbolDefinition
{
    SymbolStorageClass Class { get; }

    SymbolType Type { get; }

    uint[] Dimensions { get; }

    uint Size { get; }

    string Tag { get; }

    string Name { get; }
}