namespace psx_dump_sym;

public abstract class SymbolRecord
{
    public abstract void Write(SymbolHeader header, TextWriter writer, uint line);
}