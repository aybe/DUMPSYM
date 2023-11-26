using Whatever.Extensions;

namespace psx_dump_sym;

public class SymbolIncSldLineNumByByte(Stream stream) : Symbol
{
    public byte Length { get; } = stream.Read<byte>();

    public override string ToString()
    {
        return $"{nameof(Length)}: {Length}";
    }
}