namespace DUMPSYM;

[Serializable]
public sealed class SymbolRecordBlockStart : SymbolRecord, ISymbolFunctionBlock
{
    public SymbolRecordBlockStart()
    {
    }

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