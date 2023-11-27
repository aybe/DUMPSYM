using Whatever.Extensions;

namespace psx_dump_sym;

public sealed class SymbolRecordIncSldLineNumByWord(Stream stream) : SymbolRecord
{
    public ushort Length { get; } = stream.Read<ushort>();
}