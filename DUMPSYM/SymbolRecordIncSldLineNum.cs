namespace DUMPSYM;

[Serializable]
public sealed record SymbolRecordIncSldLineNum : SymbolRecord, ISymbolLineModifier
{
    public SymbolRecordIncSldLineNum()
    {
    }

    public SymbolRecordIncSldLineNum(SymbolContext context)
    {
        Line = context.Line += 1;
    }

    private uint Line { get; set; }

    public override string ToString()
    {
        return $"Inc SLD linenum (to {Line})";
    }
}