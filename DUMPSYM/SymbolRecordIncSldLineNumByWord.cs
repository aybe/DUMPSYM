using Whatever.Extensions;

namespace DUMPSYM;

public sealed class SymbolRecordIncSldLineNumByWord(Stream stream) : SymbolRecord
{
    public ushort Line { get; } = stream.Read<ushort>();

    public override void Write(SymbolHeader header, TextWriter writer, uint line)
    {
        header.WriteHeaderPositionAddressType(writer);

        writer.WriteLine($"Inc SLD linenum by word {Line} (to {line})");
    }
}