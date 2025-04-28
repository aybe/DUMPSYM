using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace DUMPSYM;

[Serializable]
[NoReorder]
public sealed record SymbolRecordDef2 : SymbolRecord, ISymbolDefinition2
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

    public bool Equals(SymbolRecordDef2? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Class == other.Class && Type.Equals(other.Type) && Size == other.Size && Dimensions.SequenceEqual(other.Dimensions) && Tag == other.Tag && Name == other.Name;
    }

    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public override int GetHashCode()
    {
        return HashCode.Combine(Class, Type, Size, Dimensions.Length, Dimensions.Aggregate(0, HashCode.Combine), Tag, Name);
    }

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