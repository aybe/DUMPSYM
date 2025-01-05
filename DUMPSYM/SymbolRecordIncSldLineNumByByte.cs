namespace DUMPSYM;

public sealed class SymbolRecordIncSldLineNumByByte : SymbolRecord, ISymbolLineModifier
{
    public SymbolRecordIncSldLineNumByByte(SymbolContext context) : base(context)
    {
        Increment = context.Read<byte>();
        Line = context.Line;
        context.Line += Increment;
    }

    public uint Line { get; set; }

    public byte Increment { get; }

    public override string ToString()
    {
        return $"Inc SLD linenum by byte {Increment} (to {Line + Increment})";
    }
}