namespace DUMPSYM;

public sealed class Symbol(SymbolHeader header, SymbolRecord record)
{
    public SymbolHeader Header { get; } = header;

    public SymbolRecord Record { get; } = record;

    public override string ToString()
    {
        return $"{Header} {Record}";
    }
}