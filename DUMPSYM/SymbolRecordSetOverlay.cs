namespace DUMPSYM;

public sealed class SymbolRecordSetOverlay : SymbolRecord, ISymbolOverlay
{
    public SymbolRecordSetOverlay(SymbolContext context) : base(context)
    {
    }
}