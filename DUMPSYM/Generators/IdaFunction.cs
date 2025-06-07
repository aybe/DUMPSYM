using DUMPSYM.Symbols;

namespace DUMPSYM.Generators;

public sealed class IdaFunction
{
    public required string[] Comments { get; init; }

    public required string File { get; init; }

    public required string Declaration { get; init; }

    public required SymbolHeader Header { get; init; }

    public required string Name { get; init; }

    public override string ToString()
    {
        return Name;
    }
}