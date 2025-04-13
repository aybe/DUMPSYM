namespace DUMPSYM;

[Serializable]
public sealed class SymbolRecordSetSldLineNum : SymbolRecord, ISymbolLineModifier
{
    public SymbolRecordSetSldLineNum()
    {
    }

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