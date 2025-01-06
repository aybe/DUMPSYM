namespace DUMPSYM;

public sealed class SymbolRecordDef2(SymbolContext context) : SymbolRecord, ISymbolName, ISymbolDefinition2
{
    public SymbolStorageClass Class { get; } = context.Read<SymbolStorageClass>();

    public SymbolType Type { get; } = context.Read<SymbolType>();

    public uint Size { get; } = context.Read<uint>();

    public uint[] Dimensions { get; } = ReadDimensions(context);

    public string Tag { get; } = context.ReadStringAscii();

    public string Name { get; } = context.ReadStringAscii();

    private static uint[] ReadDimensions(SymbolContext context)
    {
        var dimensions = new uint[context.Read<ushort>()];

        for (var i = 0; i < dimensions.Length; i++)
        {
            dimensions[i] = context.Read<uint>();
        }

        return dimensions;
    }

    public override string ToString()
    {
        return
            $"Def2 class {Class} type {Type} size {Size} dims {(Dimensions.Length > 0 ? $"{Dimensions.Length} {string.Join(" ", Dimensions)}" : "0")} tag {Tag} name {Name}";
    }
}