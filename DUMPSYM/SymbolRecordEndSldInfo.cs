namespace DUMPSYM;

[Serializable]
public sealed class SymbolRecordEndSldInfo : SymbolRecord, ISymbolLineModifierEnd
{
    public override string ToString()
    {
        return "End SLD info";
    }
}