namespace DUMPSYM;

public abstract class SymbolRecord(SymbolContext context)
{
    private SymbolHeader Header { get; } = context.Header;

    public override string ToString()
    {
        return "";
        return Header.ToString();
    }
}