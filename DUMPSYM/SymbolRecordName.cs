namespace DUMPSYM;

[Serializable]
public sealed class SymbolRecordName : SymbolRecord, ISymbolName
{
    public SymbolRecordName()
    {
    }

    public SymbolRecordName(SymbolContext context)
    {
        Name = context.ReadStringAscii();
    }

    public string Name { get; set; } = null!;

    public override string ToString()
    {
        return $"{Name}";
    }
}