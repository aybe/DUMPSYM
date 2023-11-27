using Whatever.Extensions;

namespace psx_dump_sym;

public sealed class SymbolRecordSetSldLineNum(Stream stream) : SymbolRecord
{
    public uint Value { get; } = stream.Read<uint>();

    public override void Write(SymbolHeader header, TextWriter writer, uint line)
    {
        header.WriteHeaderPositionAddressType(writer);

        writer.WriteLine($"Set SLD linenum to {line}");
    }
}