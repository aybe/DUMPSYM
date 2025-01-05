namespace DUMPSYM;

public sealed class SymbolRecordSetSldToLineOfFile(SymbolContext context) : SymbolRecord, ISymbolLineModifier
{
    public uint Line { get; } = context.Line = context.Read<uint>();

    public string File { get; } = context.ReadStringAscii();

    public override string ToString()
    {
        return $"Set SLD to line {Line} of file {File}";
    }
}