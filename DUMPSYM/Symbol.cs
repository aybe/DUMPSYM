namespace DUMPSYM;

public sealed class Symbol
{
    public Symbol(SymbolHeader header, SymbolRecord record)
    {
        Header = header;
        Record = record;
    }

    public SymbolHeader Header { get; }

    public SymbolRecord Record { get; }

    public override string ToString()
    {
        return $"{Header} {Record}";
    }
}