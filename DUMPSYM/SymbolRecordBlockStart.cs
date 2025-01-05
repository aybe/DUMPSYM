namespace DUMPSYM;

public sealed class SymbolRecordBlockStart : SymbolRecord, ISymbolFunctionBlock
{
    public SymbolRecordBlockStart(SymbolContext context) : base(context)
    {
        Line = context.Read<uint>();
    }

    public uint Line { get; }

    public override string ToString()
    {
        return $"Block_start  line = {Line}";
    }
}