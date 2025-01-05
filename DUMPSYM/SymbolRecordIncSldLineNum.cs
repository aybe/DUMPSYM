namespace DUMPSYM;

public sealed class SymbolRecordIncSldLineNum : SymbolRecord, ISymbolLineModifier
{
    public SymbolRecordIncSldLineNum(SymbolContext context) : base(context)
    {
        Line = context.Line += 1;
    }

    private uint Line { get; }

    public override string ToString()
    {
        return $"Inc SLD linenum (to {Line})";
    }
}