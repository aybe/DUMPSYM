using Whatever.Extensions;

namespace psx_dump_sym;

public sealed class SymbolRecordDef(Stream stream) : SymbolRecord
{
    public SymbolStorageClass Class { get; } = SymbolUtility.ReadEnum<SymbolStorageClass>(stream);

    public SymbolType Type { get; } = new(stream.Read<ushort>());

    public uint Size { get; } = stream.Read<uint>();

    public string Name { get; } = stream.ReadStringAscii(stream.Read<byte>());

    public override void Write(SymbolHeader header, TextWriter writer, uint line)
    {
        header.WriteHeaderPositionAddressType(writer);

        writer.WriteLine($"Def class {Class} type {Type} size {Size} name {Name}");
    }
}