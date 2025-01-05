namespace DUMPSYM;

public sealed class SymbolRecordBlockEnd : SymbolRecord, ISymbolFunctionBlock
{
    public SymbolRecordBlockEnd(SymbolContext context) : base(context)
    {
        Line = context.Read<uint>();
    }

    public uint Line { get; }

    public override string ToString()
    {
        return $"Block_end  line = {Line}";
    }
}