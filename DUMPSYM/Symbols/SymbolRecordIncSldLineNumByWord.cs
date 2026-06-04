namespace DUMPSYM.Symbols;

public sealed record SymbolRecordIncSldLineNumByWord : SymbolRecord, ISymbolLineModifier
{
    public SymbolRecordIncSldLineNumByWord(SymbolContext context)
    {
        Increment = context.Read<ushort>();

        Line = context.Line;

        context.Line += Increment;
    }

    public uint Line { get; }

    public ushort Increment { get; }

    public override string ToString()
    {
        return $"Inc SLD linenum by word {Increment} (to {Line + Increment})";
    }
}