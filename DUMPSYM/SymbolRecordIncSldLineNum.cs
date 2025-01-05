namespace DUMPSYM;

public sealed class SymbolRecordIncSldLineNum(SymbolContext context) : SymbolRecord, ISymbolLineModifier
{
    private uint Line { get; } = context.Line += 1;

    public override string ToString()
    {
        return $"Inc SLD linenum (to {Line})";
    }
}