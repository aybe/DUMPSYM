namespace DUMPSYM;

public sealed class SymbolRecordFunctionEnd(SymbolContext context) : SymbolRecord, ISymbolFunctionEnd
{
    public uint Line { get; } = context.Read<uint>();

    public override string ToString()
    {
        return $"Function_end   line {Line}";
    }
}