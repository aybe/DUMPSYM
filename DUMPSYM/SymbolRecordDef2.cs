using Whatever.Extensions;

namespace DUMPSYM;

public sealed class SymbolRecordDef2(Stream stream) : SymbolRecord
{
    public SymbolStorageClass Class { get; } = stream.Read<SymbolStorageClass>();

    public SymbolType Type { get; } = stream.Read<SymbolType>();

    public uint Size { get; } = stream.Read<uint>();

    public uint[] Dimensions { get; } = ReadDimensions(stream);

    public string Tag { get; } = stream.ReadStringAscii(stream.Read<byte>());

    public string Name { get; } = stream.ReadStringAscii(stream.Read<byte>());

    private static uint[] ReadDimensions(Stream stream)
    {
        var length = stream.Read<ushort>();

        var dimensions = new uint[length];

        for (var i = 0; i < length; i++)
        {
            dimensions[i] = stream.Read<uint>();
        }

        return dimensions;
    }

    public override void Write(SymbolHeader header, TextWriter writer, uint line)
    {
        header.WriteHeaderPositionAddressType(writer);

        writer.WriteLine(
            $"Def2 " +
            $"class {Class} " +
            $"type {Type} " +
            $"size {Size} " +
            $"dims {(Dimensions.Length > 0 ? $"{Dimensions.Length} {string.Join(" ", Dimensions)}" : "0")} " +
            $"tag {Tag} " +
            $"name {Name}");
    }
}