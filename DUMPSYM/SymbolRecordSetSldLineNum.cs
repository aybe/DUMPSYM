namespace DUMPSYM;

public sealed record SymbolRecordSetSldLineNum : SymbolRecord, ISymbolLineModifier
{
    public SymbolRecordSetSldLineNum(SymbolContext context)
    {
        Line = context.Line = context.Read<uint>();
    }

    public uint Line { get; set; }

    public override string ToString()
    {
        return $"Set SLD linenum to {Line}";
    }
}