namespace DUMPSYM.Symbols;

public sealed record SymbolRecordIncSldLineNumByByte : SymbolRecord, ISymbolLineModifier
{
    public SymbolRecordIncSldLineNumByByte(SymbolContext context)
    {
        Increment = context.Read<byte>();

        Line = context.Line;

        context.Line += Increment;
    }

    public uint Line { get; set; }

    public byte Increment { get; set; }

    public override string ToString()
    {
        return $"Inc SLD linenum by byte {Increment} (to {Line + Increment})";
    }
}