using Whatever.Extensions;

namespace DUMPSYM;

public sealed class SymbolRecordDef2(Stream stream) : SymbolRecord
{
    public SymbolStorageClass Class { get; } = SymbolUtility.ReadEnum<SymbolStorageClass>(stream);

    public SymbolType Type { get; } = new(stream.Read<ushort>());

    public uint Size { get; } = stream.Read<uint>();

    public uint[] Dimensions { get; } = ReadDimensions(stream);

    public string Tag { get; } = SymbolUtility.ReadStringAscii(stream);

    public string Name { get; } = SymbolUtility.ReadStringAscii(stream);

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