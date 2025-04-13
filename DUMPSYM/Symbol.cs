namespace DUMPSYM;

[Serializable]
public sealed class Symbol
{
    public Symbol()
    {
    }

    public Symbol(SymbolHeader header, SymbolRecord record)
    {
        Header = header;
        Record = record;
    }

    public SymbolHeader Header { get; set; }

    public SymbolRecord Record { get; set; } = null!;

    public override string ToString()
    {
        return $"{Header} {Record}";
    }
}