namespace DUMPSYM;

public sealed record SymbolRecordSetSldToLineOfFile : SymbolRecord, ISymbolFileStart
{
    public SymbolRecordSetSldToLineOfFile(SymbolContext context)
    {
        Line = context.Line = context.Read<uint>();
        File = context.ReadStringAscii();
    }

    public uint Line { get; set; }

    public string File { get; set; } = null!;

    public override string ToString()
    {
        return $"Set SLD to line {Line} of file {File}";
    }
}