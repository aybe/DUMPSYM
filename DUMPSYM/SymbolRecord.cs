namespace DUMPSYM;

public abstract record SymbolRecord : ISymbol // TODO make these really true records without setters
{
    public SymbolHeader Header { get; set; }

    public virtual bool Equals(SymbolRecord? other)
    {
        return true; // BUG header equality screws finding duplicate fake types

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

    public override int GetHashCode()
    {
        return 0; // BUG header equality screws finding duplicate fake types

        return Header.GetHashCode();
    }

    public override string ToString()
    {
        return string.Empty;
    }
}