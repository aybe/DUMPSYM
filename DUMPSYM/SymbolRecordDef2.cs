namespace DUMPSYM;

[Serializable]
public sealed class SymbolRecordDef2 : SymbolRecord, ISymbolDefinition2
{
    public SymbolRecordDef2()
    {
    }

    public SymbolRecordDef2(SymbolStorageClass @class, SymbolType type, uint size, uint[] dimensions, string tag, string name)
    {
        Class = @class;
        Type = type;
        Size = size;
        Dimensions = dimensions;
        Tag = tag;
        Name = name;
    }

    public SymbolRecordDef2(SymbolContext context)
    {
        Class = context.Read<SymbolStorageClass>();
        Type = context.Read<SymbolType>();
        Size = context.Read<uint>();
        Dimensions = ReadDimensions(context);
        Tag = context.ReadStringAscii();
        Name = context.ReadStringAscii();
    }

    public SymbolStorageClass Class { get; set; }

    public SymbolType Type { get; set; }

    public uint Size { get; set; }

    public uint[] Dimensions { get; set; } = null!;

    public string Tag { get; set; } = null!;

    public string Name { get; set; } = null!;

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