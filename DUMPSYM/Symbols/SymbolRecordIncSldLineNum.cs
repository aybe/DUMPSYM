namespace DUMPSYM.Symbols;

public sealed record SymbolRecordIncSldLineNum : SymbolRecord, ISymbolLineModifier
{
    public SymbolRecordIncSldLineNum(SymbolContext context)
    {
        Line = context.Line += 1;
    }

    private uint Line { get; }

    public override string ToString()
    {
        return $"Inc SLD linenum (to {Line})";
    }
}