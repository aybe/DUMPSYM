namespace DUMPSYM;

public sealed class SymbolRecordName(SymbolContext context) : SymbolRecord(context), ISymbolName
{
    public string Name { get; } = context.ReadStringAscii();

    public override string ToString()
    {
        return $"{Name}";
    }
}