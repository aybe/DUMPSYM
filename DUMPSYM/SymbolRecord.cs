namespace DUMPSYM;

public abstract class SymbolRecord
{
    public abstract void Write(SymbolHeader header, TextWriter writer, uint line);
}