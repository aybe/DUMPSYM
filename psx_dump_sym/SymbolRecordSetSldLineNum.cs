using Whatever.Extensions;

namespace psx_dump_sym;

public class SymbolRecordSetSldLineNum(Stream stream) : SymbolRecord
{
    public uint Value { get; } = stream.Read<uint>();
}