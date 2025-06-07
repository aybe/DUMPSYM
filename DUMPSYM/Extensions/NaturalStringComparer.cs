using System.Text.RegularExpressions;

namespace DUMPSYM.Tests;

public sealed class NaturalStringComparer : Comparer<string>
{
    private NaturalStringComparer(StringComparison comparison)
    {
        Comparison = comparison;
    }

    private StringComparison Comparison { get; }

    /// <remarks>
    ///     A, B, C, a, b, c.
    /// </remarks>
    public static NaturalStringComparer Ordinal { get; } = new(StringComparison.Ordinal);

    /// <summary>
    ///     A, a, B, b, C, c.
    /// </summary>
    public static NaturalStringComparer OrdinalIgnoreCase { get; } = new(StringComparison.OrdinalIgnoreCase);

    private static Regex Split { get; } = new(@"\d+|\D+", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public override int Compare(string? x, string? y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        if (x is null)
        {
            return -1;
        }

        if (y is null)
        {
            return 1;
        }

        var xe = Split.EnumerateMatches(x).GetEnumerator();
        var ye = Split.EnumerateMatches(y).GetEnumerator();

        while (true)
        {
            var xNext = xe.MoveNext();
            var yNext = ye.MoveNext();

            var xs = x.AsSpan(xe.Current.Index, xe.Current.Length);
            var ys = y.AsSpan(ye.Current.Index, ye.Current.Length);

            var i = xNext.CompareTo(yNext);

            if (i != 0)
            {
                return xNext == yNext ? i : CompareStrings(xs, ys); // e.g. _GsCOORDINATE2 then _GsCOORDINATE
            }

            if (char.IsDigit(xs[0]))
            {
                var j = CompareNumbers(xs, ys);

                if (j != 0)
                {
                    return j;
                }
            }
            else
            {
                var j = CompareStrings(xs, ys);

                if (j != 0)
                {
                    return j;
                }
            }
        }
    }

    private int CompareNumbers(ReadOnlySpan<char> x, ReadOnlySpan<char> y)
    {
        var xs = x.TrimStart('0');
        var ys = y.TrimStart('0');

        var xn = xs.Length;
        var yn = ys.Length;

        var mag = xn.CompareTo(yn);

        if (mag != 0)
        {
            return mag; // different magnitudes
        }

        var num = xs.CompareTo(ys, Comparison);

        if (num != 0)
        {
            return num; // different numbers
        }

        var xz = x.Length - xn;
        var yz = y.Length - yn;

        if (xz > yz)
        {
            return -1; // same numbers, different leading zeros
        }

        if (yz > xz)
        {
            return +1; // same numbers, different leading zeros
        }

        return 0; // same numbers
    }

    private int CompareStrings(ReadOnlySpan<char> x, ReadOnlySpan<char> y)
    {
        return x.CompareTo(y, Comparison);
    }
}