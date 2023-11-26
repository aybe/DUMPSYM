using Whatever.Extensions;

namespace psx_dump_sym;

public class SymbolRecordIncSldLineNumByByte(Stream stream) : SymbolRecord
{
    public byte Length { get; } = stream.Read<byte>();

    public override string ToString()
    {
        return $"{nameof(Length)}: {Length}";
    }
}