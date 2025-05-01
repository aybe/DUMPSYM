using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace DUMPSYM;

[Serializable]
[NoReorder]
public sealed record SymbolRecordDef : SymbolRecord, ISymbolDefinition
{
    public SymbolRecordDef()
    {
    }

    public SymbolRecordDef(SymbolStorageClass @class, SymbolType type, uint size, string name)
    {
        Class = @class;
        Type = type;
        Size = size;
        Name = name;
    }

    public SymbolRecordDef(SymbolContext context)
    {
        Class = context.Read<SymbolStorageClass>();
        Type = context.Read<SymbolType>();
        Size = context.Read<uint>();
        Name = context.ReadStringAscii();
    }

    public SymbolStorageClass Class { get; set; }

    public SymbolType Type { get; set; }

    public uint Size { get; set; }

    public string Name { get; set; } = null!;

    public bool Equals(SymbolRecordDef? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Class == other.Class && Type.Equals(other.Type) && Size == other.Size && Name == other.Name;
    }

    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public override int GetHashCode()
    {
        return HashCode.Combine(Class, Type, Size, Name);
    }

    public override string ToString()
    {
        return $"Def class {Class} type {Type} size {Size} name {Name}";
    }
}