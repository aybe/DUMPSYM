using DUMPSYM.Extensions;

namespace DUMPSYM;

[Serializable]
public sealed class Symbol
{
    public Symbol()
    {
    }

    public Symbol(SymbolHeader header, SymbolRecord record)
    {
        Header = header;
        Record = record;
    }

    public SymbolHeader Header { get; set; }

    public SymbolRecord Record { get; set; } = null!;

    public override string ToString()
    {
        return $"{Header} {Record}";
    }

    public static Symbol[][] Split(Symbol[] symbols)
    {
        var records = symbols.Select(s => s.Record).Cast<ISymbol>().ToArray();

        var split = new List<Symbol[]>();

        var searches = new SymbolSearch[]
        {
            new(s => s.IsName(), s => s.IsName()),
            new(s => s.IsExternal(), s => s.IsExternal()),
            new(s => s.IsStatic(), s => s.IsStatic()),
            new(s => s.IsTypedef(), s => s.IsTypedef()),
            new(s => s.IsFileEnd(), s => s.IsFileEnd()),
            new(s => s.IsFileHeader(), s => s.IsFileFooter()),
            new(s => s.IsFunctionHeader(), s => s.IsFunctionFooter()),
            new(s => s.IsStructHeader(), s => s.IsTypeFooter()),
            new(s => s.IsUnionHeader(), s => s.IsTypeFooter())
        };

        for (var i = 0; i < records.Length; i++)
        {
            var success = false;

            foreach (var search in searches)
            {
                if (!records.TryGetRange(i, search.Header, search.Footer, out var range))
                {
                    continue;
                }

                var slice = symbols[range];

                split.Add(slice);

                i = range.End.Value - 1;

                success = true;

                break;
            }

            if (!success)
            {
                throw new NotImplementedException($"{i} {symbols[i]}");
            }
        }

        return split.ToArray();
    }

    private sealed record SymbolSearch(Predicate<ISymbol> Header, Predicate<ISymbol> Footer);
}