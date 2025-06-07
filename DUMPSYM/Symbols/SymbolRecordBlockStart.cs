namespace DUMPSYM.Symbols;

public sealed record SymbolRecordBlockStart : SymbolRecord, ISymbolFunctionBlock
{
    public SymbolRecordBlockStart(SymbolContext context)
    {
        Line = context.Read<uint>();
    }

    public uint Line { get; set; }

    public override string ToString()
    {
        return $"Block_start  line = {Line}";
    }
}