namespace DUMPSYM;

public sealed record SymbolRecordFunctionEnd : SymbolRecord, ISymbolFunctionEnd
{
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