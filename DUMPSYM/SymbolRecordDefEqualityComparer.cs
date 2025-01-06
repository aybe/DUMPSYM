namespace DUMPSYM;

public sealed class SymbolRecordDefEqualityComparer : EqualityComparer<SymbolRecordDef>
{
    public static IEqualityComparer<SymbolRecordDef> Instance { get; } = new SymbolRecordDefEqualityComparer();

    public override bool Equals(SymbolRecordDef? x, SymbolRecordDef? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (x is null)
        {
            return false;
        }

        if (y is null)
        {
            return false;
        }

        if (x.GetType() != y.GetType())
        {
            return false;
        }

        return x.Class == y.Class && x.Type.Equals(y.Type) && x.Size == y.Size && x.Name == y.Name;
    }

    public override int GetHashCode(SymbolRecordDef obj)
    {
        return HashCode.Combine((int)obj.Class, obj.Type, obj.Size, obj.Name);
    }
}