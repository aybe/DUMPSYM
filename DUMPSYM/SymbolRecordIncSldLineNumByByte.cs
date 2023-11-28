using Whatever.Extensions;

namespace DUMPSYM;

public sealed class SymbolRecordIncSldLineNumByByte(Stream stream) : SymbolRecord, ISymbolLineModifier
{
    public byte Line { get; } = stream.Read<byte>();

    public override void Write(SymbolHeader header, TextWriter writer, uint line)
    {
        header.WriteHeaderPositionAddressType(writer);

        writer.WriteLine($"Inc SLD linenum by byte {Line} (to {line})");
    }
}