namespace psx_dump_sym;

public sealed class SymbolRecordName(Stream stream) : SymbolRecord
{
    public string Name { get; } = SymbolUtility.ReadStringAscii(stream);
}