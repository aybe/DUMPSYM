using System.Collections.Frozen;
using DUMPSYM.Symbols;

namespace DUMPSYM.Generators;

public sealed class IdaGenerator
{
    public IdaGenerator(List<Symbol> symbols, IdaHeaderGeneratorOptions options)
    {
        // symbols shall be cleaned first to produce correct output
        // functions are a trap as they contain types and typedefs
        // and other symbols are useless for generating a header

        WriteLine($"{symbols.Count} symbols found");

        WriteLine("Cleaning up symbols...");

        CleanupTypedefs(symbols, options.RemoveTypedefs);

        CleanupTypedefsUnsigned(symbols);

        WriteLine("Splitting symbols...");

        var split = Symbol.Split(symbols.ToArray()).ToList();

        WriteLine($"{split.Count} symbols found");

        var remove1 = split.RemoveAll(s => s[0].IsExternal);
        var remove2 = split.RemoveAll(s => s[0].IsFile);
        var remove3 = split.RemoveAll(s => s[0].IsFileEnd);
        var remove4 = split.RemoveAll(s => s[0].IsFunction);
        var remove5 = split.RemoveAll(s => s[0].IsStatic);
        var remove6 = split.RemoveAll(s => s[0].IsVariable);

        WriteLine($"Removed {remove1} externals");
        WriteLine($"Removed {remove2} files");
        WriteLine($"Removed {remove3} file endings");
        WriteLine($"Removed {remove4} functions");
        WriteLine($"Removed {remove5} statics");
        WriteLine($"Removed {remove6} variables");

        WriteLine($"{split.Count} symbols remaining");

        // most types and typedefs are duplicates, except for fake types
        // these are compiler-generated and often reuse the same names
        // with a special comparer, we can accurately filter them out

        var map = new SortedDictionary<int, Symbol[]>();

        var set = new HashSet<Symbol[]>(SymbolArrayEqualityComparer.MembersTypeName);

        foreach (var item in split.Where(set.Add))
        {
            map.Add(map.Count, item);
        }

        Symbols = [..map.Values.SelectMany(s => s)];

        SymbolsGroups = [..map.Values];

        var headers = map.Values.Select(s => s[0]).ToArray();

        WriteLine($"Remaining typedefs: {headers.Count(s => s.IsTypeDefinition)}");
        WriteLine($"Remaining types: {headers.Count(s => s.IsTypeHeader)}");
    }

    public Symbol[] Symbols { get; }

    public Symbol[][] SymbolsGroups { get; }

    public HashSet<Symbol> Typedefs { get; } = [];

    public FrozenDictionary<SymbolTypeKind, string> TypedefsOverrides { get; } = new Dictionary<SymbolTypeKind, string>
    {
        { SymbolTypeKind.UCHAR, "u_char" },
        { SymbolTypeKind.USHORT, "u_short" },
        { SymbolTypeKind.UINT, "u_int" },
        { SymbolTypeKind.ULONG, "u_long" },
    }.ToFrozenDictionary();

    public static Action<string?> WriteLine { get; set; } = _ => { }; // TODO use

    private static void CleanupTypedefs(List<Symbol> symbols, IEnumerable<string> typedefs)
    {
        // symbols are duplicated as many times as there are files

        foreach (var name in typedefs)
        {
            var array = symbols.Where(s => s.IsTypeDefinition && s.Name == name).ToArray();

            var count = symbols.RemoveAll(array.Contains);

            WriteLine($"Removed {count} instances of '{name}'");
        }
    }

    private void CleanupTypedefsUnsigned(List<Symbol> symbols)
    {
        // Sys III/V compat typedefs are useless as we use u_short/u_int/u_long

        CleanupTypedefs(symbols, ["ushort", "uint", "ulong"]);

        foreach (var typedef in TypedefsOverrides.Values)
        {
            WriteLine($"Added instance of '{typedef}'");
        }

        // typedefs must be inserted after a file because of how symbols are split

        var index = symbols.FindIndex(s => s.IsFileEnd);

        var array = TypedefsOverrides
            .Select(s => new Symbol(new SymbolHeader(0, 0, 0x94), new SymbolRecordDef(SymbolStorageClass.TPDEF, new SymbolType(s.Key), 0, s.Value)))
            .ToArray();

        symbols.InsertRange(index + 1, array);
    }

    public static string ToString(SymbolStorageClass value)
    {
        var s = value switch
        {
            SymbolStorageClass.AUTO    => null,
            SymbolStorageClass.EXT     => "extern",
            SymbolStorageClass.STAT    => "static",
            SymbolStorageClass.REG     => null,
            SymbolStorageClass.LABEL   => null,
            SymbolStorageClass.MOS     => null,
            SymbolStorageClass.ARG     => null,
            SymbolStorageClass.STRTAG  => "struct",
            SymbolStorageClass.MOU     => null,
            SymbolStorageClass.UNTAG   => "union",
            SymbolStorageClass.TPDEF   => "typedef",
            SymbolStorageClass.ENTAG   => "enum",
            SymbolStorageClass.MOE     => null,
            SymbolStorageClass.REGPARM => null,
            SymbolStorageClass.FIELD   => null,
            SymbolStorageClass.EOS     => null,
            SymbolStorageClass.FILE    => null,
            _                          => throw new ArgumentOutOfRangeException(nameof(value), value, null),
        };

        return s ?? throw new NotImplementedException(value.ToString());
    }

    public static string ToString(SymbolTypeKind value)
    {
        var s = value switch
        {
            SymbolTypeKind.NULL   => "null",
            SymbolTypeKind.VOID   => "void",
            SymbolTypeKind.CHAR   => "char",
            SymbolTypeKind.SHORT  => "short",
            SymbolTypeKind.INT    => "int",
            SymbolTypeKind.LONG   => "long",
            SymbolTypeKind.FLOAT  => "float",
            SymbolTypeKind.DOUBLE => "double",
            SymbolTypeKind.STRUCT => "struct",
            SymbolTypeKind.UNION  => "union",
            SymbolTypeKind.ENUM   => "enum",
            SymbolTypeKind.MOE    => null,
            SymbolTypeKind.UCHAR  => "unsigned char",
            SymbolTypeKind.USHORT => "unsigned short",
            SymbolTypeKind.UINT   => "unsigned int",
            SymbolTypeKind.ULONG  => "unsigned long",
            _                     => throw new ArgumentOutOfRangeException(nameof(value), value, null),
        };

        return s ?? throw new NotImplementedException(value.ToString());
    }
}