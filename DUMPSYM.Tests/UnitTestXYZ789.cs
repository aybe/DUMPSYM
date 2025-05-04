using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using DUMPSYM.Extensions;

namespace DUMPSYM.Tests;

[TestClass]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed class UnitTestXYZ789 : UnitTestBase
{
    [TestMethod]
    public void TestSplitByFiles()
    {
        var printFakes = false;
        var printTypes = false;
        var printTypedefs = false;

        var distinct = GetDistinctSymbols();

        var linked = new LinkedList<Symbol>(distinct.SelectMany(s => s));

        var types = distinct.Where(s => s.Is(t => t.IsType())).ToArray();

        var types109fake = GetTypesWithName(types, ".109fake", printFakes); // TODO delete this and do it from lookup

        if (printTypes)
        {
            WriteLineVar(types.Length);

            foreach (var symbols in types)
            {
                WriteLine(symbols[0]);
            }

            WriteLine();
        }

        var typedefs = distinct.Where(s => s.Is(t => t.IsTypedef())).ToArray();

        if (printTypedefs)
        {
            WriteLineVar(typedefs.Length);

            foreach (var symbols in typedefs)
            {
                WriteLine(symbols[0]);
            }
        }

        var map1 = new Dictionary<string, int>();

        var map2 = new Dictionary<Symbol, string>();

        foreach (var symbols in types109fake)
        {
            var symbol = symbols[0];

            var symbolName = ((ISymbolDefinition)symbol.Record).Name;

            if (!map1.TryGetValue(symbolName, out var nameIndex))
            {
                map1[symbolName] = nameIndex = 0;
            }

            map1[symbolName] = nameIndex + 1;

            map2.Add(symbol, $"{symbolName}_{map1[symbolName]}");
        }

        WriteLine("Symbols with duplicate names and associated typedef if any:");

        var lookup = types.ToLookup(s => ((ISymbolDefinition)s[0].Record).Name);

        foreach (var group in lookup.Where(s => s.Count() > 1))
        {
            WriteLine($"{group.Key} ({group.Count()} duplicates)");

            foreach (var symbols in group)
            {
                var find = linked.Find(symbols[0])!;

                WriteLine($"\t{find.Value}");

                for (var n = find.Next; n != null; n = n.Next)
                {
                    if (n.Value.IsTypeEnd(out var eos))
                    {
                        var b = n.Next!.Value.Record.IsTypedef2(out var typedef, s => s.Tag == eos.Tag);

                        WriteLine($"\t\ttypedef: {(b ? typedef!.Name : "NULL")}");

                        break;
                    }
                }
            }
        }
    }

    private Symbol[][] GetTypesWithName(Symbol[][] symbols, string name, bool print)
    {
        var types = FindTypesWithName(symbols, name);

        if (print)
        {
            WriteLine($"{types.Length} types with name {name}:");

            WriteLine();

            foreach (var symbol in types)
            {
                foreach (var s in symbol)
                {
                    WriteLine(s);
                }

                WriteLine();
            }
        }

        return types;
    }

    private static Symbol[][] FindTypesWithName(Symbol[][] types, string name)
    {
        return types.Where(s => s.Is(t => t.IsType(u => u.Name == name))).ToArray();
    }

    private static Symbol[][] GetDistinctSymbols()
    {
        var symbols = Sample.Default.Symbols.ToArray();

        var split = Symbol.Split(symbols);

        var distinct = split.Distinct(SymbolArrayEqualityComparer.Instance).ToArray(); // TODO this is the good one with 3 more

        return distinct;
    }
}

[SuppressMessage("ReSharper", "CommentTypo")]
public static class SymbolHelper
// TODO why does FLOATLIB.C (last file) has ~2000 names at end?
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
    public static Symbol[][] SplitByFiles(Symbol[] symbols) // TODO use it
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

public sealed class SymbolArrayEqualityComparer : EqualityComparer<Symbol[]>
{
    public static SymbolArrayEqualityComparer Instance { get; } = new();

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

        for (var i = 0; i < x.Length; i++)
        {
            var a = x[i].Record;

            var b = y[i].Record;

            if (!a.Equals(b))
            {
                return false;
            }
        }

        return true;
    }

    private bool Equals<T>(IEnumerable<T> x, IEnumerable<T> y) where T : notnull
    {
        using var a = x.GetEnumerator();
        using var b = y.GetEnumerator();

        while (a.MoveNext() && b.MoveNext())
        {
            if (!a.Current.Equals(b.Current))
            {
                return false;
            }
        }

        return !a.MoveNext() && !b.MoveNext();
    }

    public override int GetHashCode(Symbol[] obj)
    {
        return obj.GetHashCode(s => s.Record);
    }
}

public static class SymbolArrayExtensions
{
    public static bool Is(this Symbol[] symbols, Func<ISymbol, bool> predicate)
    {
        return predicate(symbols[0].Record);
    }
}

public static class HashCodeExtensions
{
    public static int GetHashCode<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        where TSource : notnull
        where TResult : notnull
    {
        var hash = new HashCode();

        foreach (var value in source)
        {
            hash.Add(selector(value).GetHashCode());
        }

        return hash.ToHashCode();
    }
}