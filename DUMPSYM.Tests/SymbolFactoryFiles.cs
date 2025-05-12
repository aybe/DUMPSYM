using System.Collections.Immutable;
using DUMPSYM.Extensions;

namespace DUMPSYM.Tests;

public sealed class SymbolFactoryFiles
{
    public SymbolFactoryFiles(SymbolFile file)
    {
        var split = file.Symbols.Split(s => s.IsFile);

        var comparer = Comparer<Symbol>.Create((x, y) => x.Header.Position.CompareTo(y.Header.Position));

        Forward = split.ToImmutableSortedDictionary(s => s.First(), s => s.ToArray(), comparer);

        Reverse = Forward
            .SelectMany(s => s.Value.Select(t => KeyValuePair.Create(t, s.Key)))
            .ToImmutableSortedDictionary(s => s.Key, s => s.Value, comparer);
    }

    /// <summary>
    ///     Dictionary from/to file/symbol.
    /// </summary>
    public ImmutableSortedDictionary<Symbol, Symbol[]> Forward { get; }

    /// <summary>
    ///     Dictionary from/to symbol/file.
    /// </summary>
    public ImmutableSortedDictionary<Symbol, Symbol> Reverse { get; }
}