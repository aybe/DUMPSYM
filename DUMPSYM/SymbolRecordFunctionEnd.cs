namespace DUMPSYM;

[Serializable]
public sealed class SymbolRecordFunctionEnd : SymbolRecord, ISymbolFunctionEnd
{
    public SymbolRecordFunctionEnd()
    {
    }

    public SymbolRecordFunctionEnd(SymbolContext context)
    {
        Line = context.Read<uint>();
    }

    public uint Line { get; set; }

    public override string ToString()
    {
        return $"Function_end   line {Line}";
    }
}