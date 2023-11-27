using Whatever.Extensions;

namespace DUMPSYM;

public sealed class SymbolRecordOverlay(Stream stream) : SymbolRecord
{
    public uint Length { get; } = stream.Read<uint>();

    public uint Id { get; } = stream.Read<uint>();

    public override void Write(SymbolHeader header, TextWriter writer, uint line)
    {
        header.WriteHeaderPositionAddress(writer);

        writer.WriteLine($"overlay length ${Length:x8} id ${Id:x}");
    }
}