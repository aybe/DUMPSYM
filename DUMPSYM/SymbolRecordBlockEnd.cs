namespace DUMPSYM;

public sealed class SymbolRecordBlockEnd(SymbolContext context) : SymbolRecord, ISymbolFunctionBlock
{
    public uint Line { get; } = context.Read<uint>();

    public override string ToString()
    {
        return $"Block_end  line = {Line}";
    }
}