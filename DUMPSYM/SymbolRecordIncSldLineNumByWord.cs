namespace DUMPSYM;

[Serializable]
public sealed class SymbolRecordIncSldLineNumByWord : SymbolRecord, ISymbolLineModifier
{
    public SymbolRecordIncSldLineNumByWord()
    {
    }

    public SymbolRecordIncSldLineNumByWord(SymbolContext context)
    {
        Increment = context.Read<ushort>();

        Line = context.Line;

        context.Line += Increment;
    }

    public uint Line { get; set; }

    public ushort Increment { get; set; }

    public override string ToString()
    {
        return $"Inc SLD linenum by word {Increment} (to {Line + Increment})";
    }
}