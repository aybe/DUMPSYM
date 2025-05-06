using System.Collections.Frozen;
using System.Text.RegularExpressions;
using DUMPSYM.Extensions;

// ReSharper disable CommentTypo

namespace DUMPSYM.Tests;

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

        SplitDistinct = Split.Distinct(SymbolArrayEqualityComparer.Everything).ToArray();

        DistinctTypes = SplitDistinct.Where(s => s[0].IsTypeHeader).ToArray();

        Assert.AreEqual(311, DistinctTypes.Length); // TODO delete

        DistinctTypesDefinitionsMap = DistinctTypes.ToFrozenDictionary(s => s, s => GetTypeDefinition(s[^1]));

        DistinctTypeDefinitions = SplitDistinct.Where(s => s[0].IsTypeDefinition).ToArray();

        Assert.AreEqual(203, DistinctTypeDefinitions.Length); // TODO delete

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

    /// <summary>
    ///     Gets distinct types throughout .SYM file.
    /// </summary>
    public Symbol[][] DistinctTypes { get; }

    /// <summary>
    ///     Gets the type definition for a type from <see cref="DistinctTypes" />.
    /// </summary>
    public FrozenDictionary<Symbol[], Symbol?> DistinctTypesDefinitionsMap { get; }

    /// <summary>
    ///     Gets distinct typedefs throughout .SYM file.
    /// </summary>
    public Symbol[][] DistinctTypeDefinitions { get; }

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

    public Symbol? GetTypeDefinition(Symbol eos)
    {
        var tag = eos.Tag!;

        Assert.IsTrue(eos.IsTypeFooter);

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

            if (symbol.IsTypeDefinition && symbol.Tag == tag && symbol.Type is { } t && !t.Modifiers.Any())
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
}