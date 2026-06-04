namespace DUMPSYM.Symbols;

public sealed record SymbolRecordSetSldToLineOfFile : SymbolRecord, ISymbolFileStart
{
    public SymbolRecordSetSldToLineOfFile(SymbolContext context)
    {
        Line = context.Line = context.Read<uint>();
        File = context.ReadStringAscii();
    }

    public uint Line { get; }

    public string File { get; }

    public override string ToString()
    {
        return $"Set SLD to line {Line} of file {File}";
    }
}