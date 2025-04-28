using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace DUMPSYM;

[Serializable]
[NoReorder]
public sealed class SymbolRecordName : SymbolRecord, ISymbolVariable, IEquatable<SymbolRecordName>
{
    public SymbolRecordName()
    {
    }

    public SymbolRecordName(SymbolContext context)
    {
        Name = context.ReadStringAscii();
    }

    public string Name { get; set; } = null!;

    public bool Equals(SymbolRecordName? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Name == other.Name;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || (obj is SymbolRecordName other && Equals(other));
    }

    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    public override string ToString()
    {
        return $"{Name}";
    }
}