using Whatever.Extensions;

namespace DUMPSYM;

public sealed class SymbolRecordDef2 : SymbolRecord, ISymbolName, ISymbolDefinition
{
    public SymbolRecordDef2(SymbolContext context) : base(context)
    {
        Class = context.Read<SymbolStorageClass>();
        Type = context.Read<SymbolType>();
        Size = context.Read<uint>();
        Dimensions = ReadDimensions(context.Stream); // TODO
        Tag = context.ReadStringAscii();
        Name = context.ReadStringAscii();
    }

    public SymbolStorageClass Class { get; }

    public SymbolType Type { get; }

    public uint Size { get; }

    public uint[] Dimensions { get; }

    public string Tag { get; }

    public string Name { get; }

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

    public override string ToString()
    {
        return $"Def2 class {Class} type {Type} size {Size} dims {(Dimensions.Length > 0 ? $"{Dimensions.Length} {string.Join(" ", Dimensions)}" : "0")} tag {Tag} name {Name}";
    }
}