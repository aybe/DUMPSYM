namespace psx_dump_sym;

public class SymbolName : Symbol
{
    public SymbolName(Stream stream)
    {
        var name = SymbolUtility.ReadStringAscii(stream);
    }
}