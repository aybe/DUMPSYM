using Whatever.Extensions;

namespace DUMPSYM;

public sealed class SymbolRecordBlockStart(Stream stream) : SymbolRecord
{
    public uint Line { get; } = stream.Read<uint>();

    public override void Write(SymbolHeader header, TextWriter writer, uint line)
    {
        header.WriteHeaderPositionAddressType(writer);

        writer.WriteLine($"Block start  line = {Line}");
    }
}