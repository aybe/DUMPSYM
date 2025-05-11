using System.Collections.Frozen;
using System.Collections.Immutable;

// ReSharper disable CommentTypo

namespace DUMPSYM.Tests;

public sealed class SymbolFactory
// TODO why does FLOATLIB.C (last file) has ~2000 names at end?
{
    public SymbolFactory(SymbolFile file)
    {
        var symbols = file.Symbols.ToArray();

        var indices = symbols.Index().ToFrozenDictionary(s => s.Item, s => s.Index);

        LineOf = GetLines(symbols);

        var split = Symbol.Split(symbols); // by kind: defs, exts, files, funcs, names, statics, types

        var splitDistinct = split.Distinct(SymbolArrayEqualityComparer.Everything).ToArray(); // without duplicates

        DistinctType = splitDistinct.Where(s => s[0].IsTypeHeader).ToFrozenSet();

        DistinctTypeDefinition = splitDistinct.Select(s => s.First()).Where(s => s.IsTypeDefinition).ToImmutableSortedSet(SymbolPositionComparer.Instance);

        DistinctTypeDefinitionMap = DistinctType.ToFrozenDictionary(s => s, s => GetTypeDefinition(s, symbols, indices));

        var comparer2 = Comparer<Symbol[]>.Create((x, y) => x[0].Header.Position.CompareTo(y[0].Header.Position));

        DistinctTypeName = DistinctType.ToImmutableSortedDictionary(s => s, GetTypeName, comparer2);

        GeneratedType = DistinctTypeDefinitionMap.Where(s => s.Key[0].HasFakeName && s.Value == null).Select(s => s.Key).ToFrozenSet();

        GeneratedTypeGroup = GeneratedType.GroupBy(s => s, SymbolArrayEqualityComparer.Members).Where(s => s.Count() > 1).ToFrozenDictionary(s => s.Key, s => s.ToFrozenSet());
    }

    /// <summary>
    ///     Set containing headers of distinct types in .SYM file.
    /// </summary>
    /// <remarks>
    ///     Types are all unique by-member, fake types may share names.
    /// </remarks>
    public FrozenSet<Symbol[]> DistinctType { get; }

    /// <summary>
    ///     Set containing distinct type definitions in .SYM file.
    /// </summary>
    /// <remarks>
    ///     Type definitions are sorted by position; they may refer to fake types that share names.
    /// </remarks>
    public ImmutableSortedSet<Symbol> DistinctTypeDefinition { get; }

    /// <summary>
    ///     Dictionary to map a distinct type to its definition, if any.
    /// </summary>
    /// <remarks>
    ///     See <see cref="DistinctType" />.
    /// </remarks>
    public FrozenDictionary<Symbol[], Symbol?> DistinctTypeDefinitionMap { get; }

    /// <summary>
    ///     Gets the real name of a distinct type.
    /// </summary>
    /// <remarks>
    ///     See <see cref="DistinctType" />.
    /// </remarks>
    public ImmutableSortedDictionary<Symbol[], string> DistinctTypeName { get; }

    /// <summary>
    ///     Set containing headers of compiler-generated types.
    /// </summary>
    /// <remarks>
    ///     Types in this set may share the same fake names (see <see cref="DistinctType" />).
    /// </remarks>
    public FrozenSet<Symbol[]> GeneratedType { get; }

    /// <summary>
    ///     Dictionary of compiler-generated types that are same by-member.
    /// </summary>
    /// <remarks>
    ///     Types in this dictionary may share the same fake names (see <see cref="GeneratedType" />).
    /// </remarks>
    public FrozenDictionary<Symbol[], FrozenSet<Symbol[]>> GeneratedTypeGroup { get; }

    /// <summary>
    ///     Dictionary to map a symbol to its corresponding line in DUMPSYM output.
    /// </summary>
    public FrozenDictionary<Symbol, int> LineOf { get; }

    private static FrozenDictionary<Symbol, int> GetLines(Symbol[] symbols)
    {
        var lines = new Dictionary<Symbol, int>(symbols.Length);

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

    private static Symbol? GetTypeDefinition(Symbol[] type, Symbol[] symbols, FrozenDictionary<Symbol, int> indices)
    {
        if (type[^1] is not { IsTypeFooter: true } eos)
        {
            throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        var span = symbols.AsSpan(indices[eos] + 1);

        foreach (var symbol in span)
        {
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

            if (symbol.IsTypeDefinition && symbol.Tag == eos.Tag && symbol.Type is { } t && !t.Modifiers.Any())
            {
                return symbol; // first match
            }
        }

        return null; // none found, fake type name should be transformed to be unique
    }

    private string GetTypeName(Symbol[] type)
    {
        if (type[0] is not { IsTypeHeader: true } hdr)
        {
            throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        var def = DistinctTypeDefinitionMap[type];

        var name = hdr.Name!;

        var safe = def?.Name ?? (hdr.HasFakeName ? $"_{name[1..]}_{hdr.Header.Position:x6}" : name);

        return safe;
    }
}