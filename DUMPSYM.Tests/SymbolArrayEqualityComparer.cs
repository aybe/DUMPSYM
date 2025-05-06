namespace DUMPSYM.Tests;

public sealed class SymbolArrayEqualityComparer : EqualityComparer<Symbol[]>
{
    private SymbolArrayEqualityComparer(Range? range = null)
    {
        Range = range ?? Range.All;
    }

    /// <summary>
    ///     Compare header/footer/members.
    /// </summary>
    public static SymbolArrayEqualityComparer Everything { get; } = new(new Range(0, ^0));

    /// <summary>
    ///     Compare members only.
    /// </summary>
    public static SymbolArrayEqualityComparer Members { get; } = new(new Range(1, ^1));

    private Range Range { get; }

    public override bool Equals(Symbol[]? x, Symbol[]? y)
    {
        if (x is null && y is null)
        {
            return true;
        }

        if (x is null || y is null)
        {
            return false;
        }

        if (x.Length != y.Length)
        {
            return false;
        }

        var r = GetSafeRange(Range, x.Length);

        var a = x[r];

        var b = y[r];

        return a.Zip(b).All(s => s.First.Record.Equals(s.Second.Record));
    }

    public override int GetHashCode(Symbol[] obj)
    {
        var code = new HashCode();

        var symbols = obj[GetSafeRange(Range, obj.Length)];

        foreach (var symbol in symbols)
        {
            code.Add(symbol.Record);
        }

        return code.ToHashCode();
    }

    private static Range GetSafeRange(Range range, int length)
    {
        var len = length - 1;

        var min = Math.Min(range.Start.Value, len);

        var max = Math.Min(range.End.Value, len);

        var rng = new Range(Index.FromStart(min), Index.FromEnd(max));

        return rng;
    }
}