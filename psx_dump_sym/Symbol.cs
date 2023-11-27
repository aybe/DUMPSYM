namespace psx_dump_sym;

public sealed class Symbol(SymbolHeader header, SymbolRecord record)
{
    public SymbolHeader Header { get; } = header;

    public SymbolRecord Record { get; } = record;
}