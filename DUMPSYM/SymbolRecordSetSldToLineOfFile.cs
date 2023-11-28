using Whatever.Extensions;

namespace DUMPSYM;

public sealed class SymbolRecordSetSldToLineOfFile(Stream stream) : SymbolRecord, ISymbolLineModifier
{
    public uint Line { get; } = stream.Read<uint>();

    public string File { get; } = stream.ReadStringAscii(stream.Read<byte>());

    public override void Write(SymbolHeader header, TextWriter writer, uint line)
    {
        header.WriteHeaderPositionAddressType(writer);

        writer.WriteLine($"Set SLD to line {Line} of file {File}");
    }
}