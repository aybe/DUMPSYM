namespace psx_dump_sym;

public class SymbolRecordName : SymbolRecord
{
    public SymbolRecordName(Stream stream)
    {
        var name = SymbolUtility.ReadStringAscii(stream);
    }
}