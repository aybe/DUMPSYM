using Whatever.Extensions;

namespace psx_dump_sym;

public sealed class SymbolRecordIncSldLineNumByWord(Stream stream) : SymbolRecord
{
    public ushort Length { get; } = stream.Read<ushort>();

    public override void Write(SymbolHeader header, TextWriter writer, uint line)
    {
        header.WriteHeaderPositionAddressType(writer);

        writer.WriteLine($"Inc SLD linenum by word {Length} (to {line})");
    }
}