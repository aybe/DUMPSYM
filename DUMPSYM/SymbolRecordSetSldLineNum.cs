using Whatever.Extensions;

namespace DUMPSYM;

public sealed class SymbolRecordSetSldLineNum(Stream stream) : SymbolRecord, ISymbolLineModifier
{
    public uint Line { get; } = stream.Read<uint>();

    public override void Write(SymbolHeader header, TextWriter writer, uint line)
    {
        header.WriteHeaderPositionAddressType(writer);

        writer.WriteLine($"Set SLD linenum to {line}");
    }
}