using Whatever.Extensions;

namespace DUMPSYM;

public sealed class SymbolRecordIncSldLineNumByByte(Stream stream) : SymbolRecord
{
    public byte Length { get; } = stream.Read<byte>();

    public override void Write(SymbolHeader header, TextWriter writer, uint line)
    {
        header.WriteHeaderPositionAddressType(writer);

        writer.WriteLine($"Inc SLD linenum by byte {Length} (to {line})");
    }
}