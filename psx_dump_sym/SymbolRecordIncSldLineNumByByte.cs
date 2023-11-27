using Whatever.Extensions;

namespace psx_dump_sym;

public sealed class SymbolRecordIncSldLineNumByByte(Stream stream) : SymbolRecord
{
    public byte Length { get; } = stream.Read<byte>();

    public override void Write(SymbolHeader header, TextWriter writer, uint line)
    {
        header.WriteHeaderPositionAddressType(writer);

        writer.WriteLine($"Inc SLD linenum by byte {Length} (to {line})");
    }
}