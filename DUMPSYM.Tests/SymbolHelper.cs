using System.Diagnostics;
using DUMPSYM.Extensions;

namespace DUMPSYM.Tests;

public static class SymbolHelper
{
    private static List<int> FindIndices<T>(T[] array, Predicate<T> predicate)
    {
        var current = 0;

        var indices = new List<int>();

        while (true)
        {
            current = Array.FindIndex(array, current, predicate);

            if (current == -1)
            {
                break;
            }

            indices.Add(current);

            current++;
        }

        return indices;
    }

    private static T[][] Split<T>(T[] array, Predicate<T> predicate)
    {
        var indices = FindIndices(array, predicate);

        var count = indices.Count;

        indices.Add(array.Length);

        var split = new T[count][];

        for (var i = 0; i < count; i++)
        {
            split[i] = array[indices[i]..indices[i + 1]];
        }

        return split;
    }

    /// <summary>
    ///     Split by <see cref="SymbolRecordSetSldToLineOfFile" />.
    /// </summary>
    public static Symbol[][] SplitByFiles(Symbol[] symbols)
    {
        return Split(symbols, s => s.Record.IsFileHeader());
    }

    /// <summary>
    ///     Trim anything past 2nd <see cref="SymbolRecordEndSldInfo" /> if any.
    /// </summary>
    [Obsolete("Despite having duplicate symbols there, some are unique.")]
    private static Symbol[] TrimFileEnd(Symbol[] symbols)
    {
        var indices = FindIndices(symbols, s => s.Record.IsFileEnd());

        var join = string.Join(", ", indices);

        var eof = indices.Count > 1 ? indices[1] : symbols.Length;

        var slice = symbols[..eof];

        Debug.WriteLine($"{symbols[0].Record}, Length: {symbols.Length}, Indices: {join}, EOF: {eof}, Names: {slice.Any(s => s.Record.IsName())}");

        return slice;
    }
}