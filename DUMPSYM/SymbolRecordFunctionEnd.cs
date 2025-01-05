namespace DUMPSYM;

public sealed class SymbolRecordFunctionEnd : SymbolRecord, ISymbolFunctionEnd
{
    public SymbolRecordFunctionEnd(SymbolContext context) : base(context)
    {
        Line = context.Read<uint>();
    }

    public uint Line { get; }

    public override string ToString()
    {
        return $"Function_end   line {Line}";
    }
}