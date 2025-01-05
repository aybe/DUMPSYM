namespace DUMPSYM;

public sealed class SymbolRecordBlockStart(SymbolContext context) : SymbolRecord, ISymbolFunctionBlock
{
    public uint Line { get; } = context.Read<uint>();

    public override string ToString()
    {
        return $"Block_start  line = {Line}";
    }
}