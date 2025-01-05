namespace DUMPSYM;

public sealed class SymbolRecordSetSldToLineOfFile : SymbolRecord, ISymbolLineModifier
{
    public SymbolRecordSetSldToLineOfFile(SymbolContext context) : base(context)
    {
        Line = context.Read<uint>();
        File = context.ReadStringAscii();

        context.Line = Line;
    }

    public uint Line { get; }

    public string File { get; }

    public override string ToString()
    {
        return $"Set SLD to line {Line} of file {File}";
    }
}