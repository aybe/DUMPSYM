namespace DUMPSYM.Symbols;

public sealed record SymbolRecordFunctionEnd : SymbolRecord, ISymbolFunctionEnd
{
    public SymbolRecordFunctionEnd(SymbolContext context)
    {
        Line = context.Read<uint>();
    }

    public uint Line { get; }

    public override string ToString()
    {
        return $"Function end   line {Line}";
    }
}