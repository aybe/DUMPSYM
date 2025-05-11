namespace DUMPSYM;

public sealed class SymbolPositionComparer : Comparer<Symbol>
{
    public static SymbolPositionComparer Instance { get; } = new();

    public override int Compare(Symbol? x, Symbol? y)
    {
        if (x is null && y is null)
        {
            return 0;
        }

        if (x is null)
        {
            return -1;
        }

        if (y is null)
        {
            return +1;
        }

        return x.Header.Position.CompareTo(y.Header.Position);
    }
}