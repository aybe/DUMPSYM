using Whatever.Extensions;

namespace psx_dump_sym;

public sealed class SymbolRecordIncSldLineNumByByte(Stream stream) : SymbolRecord
{
    public byte Length { get; } = stream.Read<byte>();
}