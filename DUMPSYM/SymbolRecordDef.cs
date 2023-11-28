using Whatever.Extensions;

namespace DUMPSYM;

public sealed class SymbolRecordDef(Stream stream) : SymbolRecord, ISymbolName, ISymbolDefinition
{
    public SymbolStorageClass Class { get; } = stream.Read<SymbolStorageClass>();

    public SymbolType Type { get; } = stream.Read<SymbolType>();

    public uint Size { get; } = stream.Read<uint>();

    uint[] ISymbolDefinition.Dimensions => Array.Empty<uint>(); // TODO see if there's a better way to do this

    string ISymbolDefinition.Tag => string.Empty; // TODO see if there's a better way to do this

    public string Name { get; } = stream.ReadStringAscii(stream.Read<byte>());

    public override void Write(SymbolHeader header, TextWriter writer, uint line)
    {
        header.WriteHeaderPositionAddressType(writer);

        writer.WriteLine($"Def class {Class} type {Type} size {Size} name {Name}");
    }
}