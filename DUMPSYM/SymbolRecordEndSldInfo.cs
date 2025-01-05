namespace DUMPSYM;

public sealed class SymbolRecordEndSldInfo : SymbolRecord
{
    public SymbolRecordEndSldInfo(SymbolContext context) : base(context)
    {
    }

    public override string ToString()
    {
        return $"End SLD info";
    }
}