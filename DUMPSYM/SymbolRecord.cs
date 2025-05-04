using System.Diagnostics.CodeAnalysis;

namespace DUMPSYM;

public abstract record SymbolRecord : ISymbol // TODO make these really true records without setters
{
    public SymbolHeader Header { get; set; }

    public virtual bool Equals(SymbolRecord? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Header.Equals(other.Header);
    }

    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public override int GetHashCode()
    {
        return Header.GetHashCode();
    }

    public override string ToString()
    {
        return string.Empty;
    }
}