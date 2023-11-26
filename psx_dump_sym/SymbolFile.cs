namespace psx_dump_sym;

public sealed class SymbolFile(string header, int version, int targetUnit, LinkedList<Symbol> symbols)
{
    public string Header { get; } = header;

    public int Version { get; } = version;

    public int TargetUnit { get; } = targetUnit;

    public LinkedList<Symbol> Symbols { get; } = symbols;
}