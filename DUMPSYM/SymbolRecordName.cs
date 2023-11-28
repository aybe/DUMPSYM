using Whatever.Extensions;

namespace DUMPSYM;

public sealed class SymbolRecordName(Stream stream) : SymbolRecord, ISymbolName
{
    public string Name { get; } = stream.ReadStringAscii(stream.Read<byte>());

    public override void Write(SymbolHeader header, TextWriter writer, uint line)
    {
        header.WriteHeaderPositionAddressType(writer);

        writer.WriteLine($"{Name}");
    }
}