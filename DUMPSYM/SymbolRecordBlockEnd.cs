namespace DUMPSYM;

public sealed record SymbolRecordBlockEnd : SymbolRecord, ISymbolFunctionBlock
{
    public SymbolRecordBlockEnd(SymbolContext context)
    {
        Line = context.Read<uint>();
    }

    public uint Line { get; set; }

    public override string ToString()
    {
        return $"Block_end  line = {Line}";
    }
}