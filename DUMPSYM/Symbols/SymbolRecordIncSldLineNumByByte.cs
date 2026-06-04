namespace DUMPSYM.Symbols;

public sealed record SymbolRecordIncSldLineNumByByte : SymbolRecord, ISymbolLineModifier
{
    public SymbolRecordIncSldLineNumByByte(SymbolContext context)
    {
        Increment = context.Read<byte>();

        Line = context.Line;

        context.Line += Increment;
    }

    public uint Line { get; }

    public byte Increment { get; }

    public override string ToString()
    {
        return $"Inc SLD linenum by byte {Increment} (to {Line + Increment})";
    }
}