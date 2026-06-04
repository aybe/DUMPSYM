namespace DUMPSYM.Symbols;

public sealed record SymbolRecordName : SymbolRecord, ISymbolVariable
{
    public SymbolRecordName(SymbolContext context)
    {
        Name = context.ReadStringAscii();
    }

    public string Name { get; }

    public override string ToString()
    {
        return Name;
    }
}