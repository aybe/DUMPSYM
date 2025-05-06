using System.Collections.Frozen;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using DUMPSYM.Extensions;

// ReSharper disable CommentTypo

namespace DUMPSYM.Tests;

[TestClass]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed class UnitTestXYZ789 : UnitTestBase
{
    private SymbolFactory Factory { get; } = new(Sample.Default);

    [TestMethod]
    public void TestSplitByFiles()
    {
        var printTypes = false;
        var printTypedefs = false;

        var types = Factory.SplitDistinct.Where(s => s[0].IsTypeHeader).ToArray();

        if (printTypes)
        {
            WriteLineVar(types.Length);

            foreach (var symbols in types)
            {
                WriteLine(symbols[0]);
            }

            WriteLine();
        }

        var typedefs = Factory.SplitDistinct.Where(s => s[0].IsTypeDefinition).ToArray();

        if (printTypedefs)
        {
            WriteLineVar(typedefs.Length);

            foreach (var symbols in typedefs)
            {
                WriteLine(symbols[0]);
            }
        }

        var showDuplicates = true;
        var showUniques = true;

        var lookup = types
            .ToLookup(s => s[0].Name)
            .Where(s => (showDuplicates && s.Count() > 1) || (showUniques && s.Count() == 1))
            .ToArray();

        WriteLine($"{lookup.Length} types with resolved names, {nameof(showDuplicates)} = {showDuplicates}, {nameof(showUniques)} = {showUniques}:");

        var sb = new StringBuilder();

        sb.AppendLine("| Line | Type | Name |");
        sb.AppendLine("|------|------|------|");

        const string path = @"C:\Files\GitHub\! PSX\DUMPSYM\MAIN.SYM.txt"; // TODO as parameter

        var uri = new Uri(path).AbsoluteUri.Replace("file:///", "vscode://file/");

        foreach (var group in lookup)
        {
            WriteLine($"{group.Key} ({group.Count()})");

            foreach (var symbols in group)
            {
                var hdr = symbols[0];

                var eos = symbols[^1];

                WriteLine($"\t{hdr}");

                var name = Factory.GetTypeName(hdr, eos, out var def);

                WriteLine($"\t\t{name}");

                var ln = Factory.LineOf(hdr);

                var c1 = $"[{hdr}]({uri}:{ln})";

                var c2 = def == null ? name : $"[{def.Name}]({uri}:{Factory.LineOf(def)})";

                sb.AppendLine($"| {ln} | {c1} | {c2} |");
            }
        }

        File.WriteAllText(Path.ChangeExtension(path, "md"), sb.ToString());
    }
}

public sealed class SymbolFactory
// TODO why does FLOATLIB.C (last file) has ~2000 names at end?
{
    public SymbolFactory(SymbolFile file)
    {
        Symbols = file.Symbols.ToArray();

        SymbolsList = new LinkedList<Symbol>([..Symbols]);

        SymbolsMap = SymbolsList.Traverse().ToDictionary(s => s.Value, s => s).ToFrozenDictionary();

        Lines = GetLines(Symbols);

        Split = Symbol.Split(Symbols);

        SplitDistinct = Split.Distinct(SymbolArrayEqualityComparer.Instance).ToArray();

        Assert.AreEqual(SymbolsList.Count, SymbolsMap.Count);
    }

    /// <summary>
    ///     Symbols as an array.
    /// </summary>
    public Symbol[] Symbols { get; }

    /// <summary>
    ///     Symbols as a linked list.
    /// </summary>
    public LinkedList<Symbol> SymbolsList { get; }

    /// <summary>
    ///     Symbols dictionary from/to symbol/linked list node.
    /// </summary>
    public FrozenDictionary<Symbol, LinkedListNode<Symbol>> SymbolsMap { get; }

    /// <summary>
    ///     Symbols dictionary from/to symbol/line.
    /// </summary>
    public FrozenDictionary<Symbol, int> Lines { get; }

    /// <summary>
    ///     Symbols split by kind.
    /// </summary>
    public Symbol[][] Split { get; }

    /// <summary>
    ///     Symbols split by kind, distinct.
    /// </summary>
    public Symbol[][] SplitDistinct { get; }

    private static Regex RegexFakeName { get; } = new(@"^\.\d+fake$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static FrozenDictionary<Symbol, int> GetLines(Symbol[] symbols)
    {
        var dictionary = new Dictionary<Symbol, int>();

        var index = 4;

        foreach (var symbol in symbols)
        {
            var s = symbol.ToString();

            using var reader = new StringReader(s);

            var count = 0;

            while (true)
            {
                var line = reader.ReadLine();

                if (line == null)
                {
                    break;
                }

                count++;
            }

            dictionary[symbol] = index;

            index += count;
        }

        return dictionary.ToFrozenDictionary();
    }

    public static string GetSafeName(string name)
    {
        return HasFakeName(name) ? $"_{name[1..]}" : name;
    }

    public static bool HasFakeName(string name)
    {
        return RegexFakeName.IsMatch(name);
    }

    private Symbol? GetTypeDefinition(Symbol eos)
    {
        var tag = eos.Tag!;

        Assert.IsTrue(eos.IsTypeFooter && HasFakeName(tag));

        var node = SymbolsMap[eos];

        for (var n = node.Next; n != null; n = n.Next)
        {
            var symbol = n.Value;

            if (symbol.IsFunction)
            {
                // functions may have an appropriate typedef, but it makes no sense to peek into them:
                // 149716: $00000000 94 Def class STRTAG type STRUCT size 3 name .109fake
                // 14972c: $00000000 94 Def class MOS type UCHAR size 0 name Red
                // 14973d: $00000001 94 Def class MOS type UCHAR size 0 name Green
                // 149750: $00000002 94 Def class MOS type UCHAR size 0 name Blue
                // 149762: $00000003 96 Def2 class EOS type NULL size 3 dims 0 tag .109fake name.eos
                // ...
                // 14b385: $800420cc 8c Function_start
                // ...
                // 14b41b: $00000000 96 Def2 class TPDEF type STRUCT size 3 dims 0 tag .109fake name Palette
                return null;
            }

            if (symbol.IsTypeHeader)
            {
                return null; // another type can use the same fake name at any time
            }

            if (symbol.IsTypeDefinition && symbol.Tag == tag)
            {
                return symbol; // first match
            }
        }

        return null; // none found, fake type name should be transformed to be unique
    }

    public string GetTypeName(Symbol hdr, Symbol eos, out Symbol? def)
    {
        def = null;

        var typeName = hdr.Name!;

        if (!HasFakeName(typeName))
        {
            return typeName;
        }

        Assert.IsTrue(hdr.IsTypeHeader);

        Assert.IsTrue(eos.IsTypeFooter);

        def = GetTypeDefinition(eos);

        var safeName = def?.Name ?? (HasFakeName(typeName) ? $"{GetSafeName(typeName)}_{hdr.Header.Position:x6}" : typeName);

        return safeName;
    }

    public int LineOf(Symbol symbol)
    {
        return Lines[symbol];
    }

    private sealed class SymbolArrayEqualityComparer : EqualityComparer<Symbol[]>
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

                if (a.Equals(b))
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        public override int GetHashCode(Symbol[] obj)
        {
            return obj.GetHashCode(s => s.Record);
        }
    }

    private static class SymbolHelper
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