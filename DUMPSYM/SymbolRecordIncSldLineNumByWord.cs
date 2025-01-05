using Whatever.Extensions;

namespace DUMPSYM;

public sealed class SymbolRecordIncSldLineNumByWord : SymbolRecord, ISymbolLineModifier
{
    public SymbolRecordIncSldLineNumByWord(SymbolContext context) : base(context)
    {
        Increment = context.Read<ushort>();

        Line = context.Line;

        context.Line += Increment;
    }

    public uint Line { get; set; }

    public ushort Increment { get; }

    public override string ToString()
    {
        return $"Inc SLD linenum by word {Increment} (to {Line + Increment})";
    }
}