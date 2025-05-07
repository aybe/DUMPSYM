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

        DistinctTypeToTypeDefinition = DistinctTypes.ToFrozenDictionary(s => s, s => GetTypeDefinition(s[^1]));

        CompilerGeneratedTypes = DistinctTypeToTypeDefinition.Where(s => HasFakeName(s.Key[0].Name!) && s.Value == null).Select(s => s.Key).ToArray();

        CompilerGeneratedTypesDuplicates = CompilerGeneratedTypes.GroupBy(s => s, SymbolArrayEqualityComparer.Members).Where(s => s.Count() > 1).ToArray();

        DistinctTypeDefinitions = SplitDistinct.Select(s => s.First()).Where(s => s.IsTypeDefinition).ToArray();

        Assert.AreEqual(203, DistinctTypeDefinitions.Length); // TODO delete

        Assert.AreEqual(SymbolsList.Count, SymbolsMap.Count);
    }

    /// <summary>
    ///     Symbols in original form, one-to-one relationship with DUMPSYM output.
    /// </summary>
    public Symbol[] Symbols { get; }

    /// <summary>
    ///     <see cref="Symbols" /> as a linked list.
    /// </summary>
    public LinkedList<Symbol> SymbolsList { get; }

    /// <summary>
    ///     Dictionary to map a symbol to its corresponding linked list node.
    /// </summary>
    public FrozenDictionary<Symbol, LinkedListNode<Symbol>> SymbolsMap { get; }

    /// <summary>
    ///     Dictionary to map a symbol to its corresponding line in DUMPSYM output.
    /// </summary>
    public FrozenDictionary<Symbol, int> Lines { get; }

    /// <summary>
    ///     Symbols split by kind: definitions, externals, files, functions, names, statics, types.
    /// </summary>
    public Symbol[][] Split { get; }

    /// <summary>
    ///     <see cref="Split" /> without duplicates.
    /// </summary>
    public Symbol[][] SplitDistinct { get; }

    /// <summary>
    ///     Arrays containing headers of compiler-generated types in <see cref="DistinctTypes" />.
    /// </summary>
    /// <remarks>
    ///     Types in these arrays may share the same fake names.
    /// </remarks>
    public Symbol[][] CompilerGeneratedTypes { get; }

    /// <summary>
    ///     Groupings of <see cref="CompilerGeneratedTypes" /> where types are same by-member.
    /// </summary>
    /// <remarks>
    ///     Types in these groupings may share the same fake names.
    /// </remarks>
    public IGrouping<Symbol[], Symbol[]>[] CompilerGeneratedTypesDuplicates { get; }

    /// <summary>
    ///     Array containing headers of distinct types in the .SYM file.
    /// </summary>
    /// <remarks>
    ///     Types are all different by-member, fake types may share the same fake names.
    /// </remarks>
    public Symbol[][] DistinctTypes { get; }

    /// <summary>
    ///     Dictionary to map a type from <see cref="DistinctTypes" /> to its corresponding type definition, if any.
    /// </summary>
    public FrozenDictionary<Symbol[], Symbol?> DistinctTypeToTypeDefinition { get; }

    /// <summary>
    ///     Array containing distinct type definitions in the .SYM file.
    /// </summary>
    /// <remarks>
    ///     Type definitions of types may refer to fake types that share the same fake names.
    /// </remarks>
    public Symbol[] DistinctTypeDefinitions { get; }

    private static Regex RegexFakeName { get; } = new(@"^\.\d+fake$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static FrozenDictionary<Symbol, int> GetLines(IEnumerable<Symbol> symbols)
    {
        var lines = new Dictionary<Symbol, int>();

        var index = 4;

        foreach (var symbol in symbols)
        {
            using var reader = new StringReader(symbol.ToString());

            var count = 0;

            while (reader.ReadLine() is not null)
            {
                count++;
            }

            lines[symbol] = index;

            index += count;
        }

        return lines.ToFrozenDictionary();
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