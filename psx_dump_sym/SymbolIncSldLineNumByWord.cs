using Whatever.Extensions;

namespace psx_dump_sym;

public class SymbolIncSldLineNumByWord(Stream stream) : Symbol
{
    public ushort Length { get; } = stream.Read<ushort>();

    public override string ToString()
    {
        return $"{nameof(Length)}: {Length}";
    }
}