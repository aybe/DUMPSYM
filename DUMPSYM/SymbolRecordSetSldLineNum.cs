namespace DUMPSYM;

public sealed class SymbolRecordSetSldLineNum(SymbolContext context) : SymbolRecord, ISymbolLineModifier
{
    public uint Line { get; } = context.Line = context.Read<uint>();

    public override string ToString()
    {
        return $"Set SLD linenum to {Line}";
    }
}