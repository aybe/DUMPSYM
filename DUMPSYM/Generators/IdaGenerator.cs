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

        Console.WriteLine($"{symbols.Count} symbols found");

        Console.WriteLine("Cleaning up symbols...");

        CleanupTypedefs(symbols, options.RemoveTypedefs);

        CleanupTypedefsUnsigned(symbols);

        Console.WriteLine("Splitting symbols...");

        var split = Symbol.Split(symbols.ToArray()).ToList();

        Console.WriteLine($"{split.Count} symbols found");

        var remove1 = split.RemoveAll(s => s[0].IsExternal);
        var remove2 = split.RemoveAll(s => s[0].IsFile);
        var remove3 = split.RemoveAll(s => s[0].IsFileEnd);
        var remove4 = split.RemoveAll(s => s[0].IsFunction);
        var remove5 = split.RemoveAll(s => s[0].IsStatic);
        var remove6 = split.RemoveAll(s => s[0].IsVariable);

        Console.WriteLine($"Removed {remove1} externals");
        Console.WriteLine($"Removed {remove2} files");
        Console.WriteLine($"Removed {remove3} file endings");
        Console.WriteLine($"Removed {remove4} functions");
        Console.WriteLine($"Removed {remove5} statics");
        Console.WriteLine($"Removed {remove6} variables");

        Console.WriteLine($"{split.Count} symbols remaining");

        // most types and typedefs are duplicates, except for fake types
        // these are compiler-generated and often reuse the same names
        // with a special comparer, we can accurately filter them out

        var map = new SortedDictionary<int, Symbol[]>();

        var set = new HashSet<Symbol[]>(SymbolArrayEqualityComparer.MembersTypeName);

        foreach (var item in split.Where(set.Add))
        {
            map.Add(map.Count, item);
        }

        Console.WriteLine($"{set.Count} unique typedefs/types found");

        Symbols = [..map.Values.SelectMany(s => s)];

        SymbolsGroups = [..map.Values];
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

    private static void CleanupTypedefs(List<Symbol> symbols, IEnumerable<string> typedefs)
    {
        // symbols are duplicated as many times as there are files

        foreach (var name in typedefs)
        {
            var array = symbols.Where(s => s.IsTypeDefinition && s.Name == name).ToArray();

            var count = symbols.RemoveAll(array.Contains);

            Console.WriteLine($"Removed {count} instances of '{name}'");
        }
    }

    private void CleanupTypedefsUnsigned(List<Symbol> symbols)
    {
        // Sys III/V compat typedefs are useless as we use u_short/u_int/u_long

        CleanupTypedefs(symbols, ["ushort", "uint", "ulong"]);

        foreach (var typedef in TypedefsOverrides.Values)
        {
            Console.WriteLine($"Added instance of '{typedef}'");
        }

        // typedefs must be inserted after a file because of how symbols are split

        var index = symbols.FindIndex(s => s.IsFileEnd);

        var array = TypedefsOverrides
            .Select(s => new Symbol(new SymbolHeader { Type = 0x94 }, new SymbolRecordDef(SymbolStorageClass.TPDEF, new SymbolType(s.Key), 0, s.Value)))
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
            SymbolTypeKind.MOE    => "", // TODO enum ends with , not ;
            SymbolTypeKind.UCHAR  => "unsigned char",
            SymbolTypeKind.USHORT => "unsigned short",
            SymbolTypeKind.UINT   => "unsigned int",
            SymbolTypeKind.ULONG  => "unsigned long",
            _                     => throw new ArgumentOutOfRangeException(nameof(value), value, null),
        };

        return s ?? throw new NotImplementedException(value.ToString());
    }
}