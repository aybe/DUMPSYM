using Whatever.Extensions;

namespace psx_dump_sym;

public sealed class SymbolRecordBlockStart(Stream stream) : SymbolRecord
{
    public uint Line { get; } = stream.Read<uint>();

    public override void Write(SymbolHeader header, TextWriter writer, uint line)
    {
        header.WriteHeaderPositionAddressType(writer);

        writer.WriteLine($"Block start  line = {Line}");
    }
}