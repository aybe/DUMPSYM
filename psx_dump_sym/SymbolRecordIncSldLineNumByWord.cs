using Whatever.Extensions;

namespace psx_dump_sym;

public class SymbolRecordIncSldLineNumByWord(Stream stream) : SymbolRecord
{
    public ushort Length { get; } = stream.Read<ushort>();

    public override string ToString()
    {
        return $"{nameof(Length)}: {Length}";
    }
}