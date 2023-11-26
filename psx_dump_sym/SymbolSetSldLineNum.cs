using Whatever.Extensions;

namespace psx_dump_sym;

public class SymbolSetSldLineNum(Stream stream) : Symbol
{
    public uint Value { get; } = stream.Read<uint>();
}