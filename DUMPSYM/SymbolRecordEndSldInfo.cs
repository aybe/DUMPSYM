namespace DUMPSYM;

[Serializable]
public sealed class SymbolRecordEndSldInfo : SymbolRecord, ISymbolFileEnd
{
    public override string ToString()
    {
        return "End SLD info";
    }
}