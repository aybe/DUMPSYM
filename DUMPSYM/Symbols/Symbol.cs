using System.Text.RegularExpressions;
using DUMPSYM.Extensions;

namespace DUMPSYM.Symbols;

public sealed partial class Symbol(SymbolHeader header, SymbolRecord record)
{
    public SymbolHeader Header { get; } = header;

    public SymbolRecord Record { get; } = record;

    public override string ToString()
    {
        return $"{Header} {Record}";
    }

    public static Symbol[][] Split(Symbol[] symbols)
    {
        var split = new List<Symbol[]>();

        var searches = new (Predicate<ISymbol> Header, Predicate<ISymbol> Footer)[]
        {
            new(s => s.IsVariable, s => s.IsVariable),
            new(s => s.IsExternal, s => s.IsExternal),
            new(s => s.IsStatic, s => s.IsStatic),
            new(s => s.IsTypedef, s => s.IsTypedef),
            new(s => s.IsFileFooter, s => s.IsFileFooter),
            new(s => s.IsFileHeader, s => s.IsFileFooter),
            new(s => s.IsFunctionHeader, s => s.IsFunctionFooter),
            new(s => s.IsEnumHeader, s => s.IsTypeFooter),
            new(s => s.IsStructHeader, s => s.IsTypeFooter),
            new(s => s.IsUnionHeader, s => s.IsTypeFooter),
        };

        var records = symbols.Select(s => s.Record).Cast<ISymbol>().ToArray();

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
}

public sealed partial class Symbol
{
    private static Regex RegexFakeName { get; } = new(@"^\.\d+fake$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    #region ISymbolDefinition

    public SymbolStorageClass? Class => Record is ISymbolDefinition d ? d.Class : null;

    public SymbolType? Type => Record is ISymbolDefinition d ? d.Type : null;

    public uint? Size => Record is ISymbolDefinition d ? d.Size : null;

    public string? Name => Record is ISymbolDefinition d ? d.Name : null;

    #endregion

    #region ISymbolDefinition2

    public uint[]? Dimensions => Record is ISymbolDefinition2 d ? d.Dimensions : null;

    public string? Tag => Record is ISymbolDefinition2 d ? string.IsNullOrEmpty(d.Tag) ? null : d.Tag : null;

    #endregion

    #region Extras

    public bool HasFakeName => !string.IsNullOrEmpty(Name) && RegexFakeName.IsMatch(Name);

    public bool HasFakeTag => !string.IsNullOrEmpty(Tag) && RegexFakeName.IsMatch(Tag);

    public bool IsExternal => Class is SymbolStorageClass.EXT;

    public bool IsFile => Record is ISymbolFileStart;

    public bool IsFileEnd => Record is ISymbolFileEnd;

    public bool IsFunction => Record is ISymbolFunction;

    public bool IsStatic => Class is SymbolStorageClass.STAT;

    public bool IsTypeDefinition => Class is SymbolStorageClass.TPDEF;

    public bool IsTypeHeader => Class is SymbolStorageClass.ENTAG or SymbolStorageClass.STRTAG or SymbolStorageClass.UNTAG;

    public bool IsVariable => Record is ISymbolVariable;

    #endregion
}