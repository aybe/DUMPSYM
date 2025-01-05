namespace DUMPSYM;

public sealed class SymbolRecordName(SymbolContext context) : SymbolRecord, ISymbolName
{
    public string Name { get; } = context.ReadStringAscii();

    public override string ToString()
    {
        return $"{Name}";
    }
}