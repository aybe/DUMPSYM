namespace DUMPSYM;

[Serializable]
public sealed record SymbolRecordEndSldInfo : SymbolRecord, ISymbolFileEnd
{
    public override string ToString()
    {
        return "End SLD info";
    }
}