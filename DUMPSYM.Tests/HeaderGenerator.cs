using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable IdentifierTypo

namespace DUMPSYM.Tests;

public sealed class HeaderGenerator : IDisposable
// generating IDA-friendly code requires to not use typedef struct/union at all
// else, IDA will rename every type with a fake name and add an extra typedef
{
    public HeaderGenerator(List<Symbol> symbols, HeaderGeneratorOptions options)
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

    private Symbol[] Symbols { get; }

    private Symbol[][] SymbolsGroups { get; }

    private HashSet<Symbol> Typedefs { get; } = [];

    private Dictionary<SymbolTypeKind, string> TypedefsOverrides { get; } = new()
    {
        { SymbolTypeKind.UCHAR, "u_char" },
        { SymbolTypeKind.USHORT, "u_short" },
        { SymbolTypeKind.UINT, "u_int" },
        { SymbolTypeKind.ULONG, "u_long" },
    };

    private IndentedTextWriter Writer { get; } = new(new StringWriter());

    public void Dispose()
    {
        Writer.Dispose();
    }

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

    public string Generate()
    {
        // as a type may generate its own typedef, keep track of them

        foreach (var symbols in SymbolsGroups)
        {
            var symbol = symbols[0];

            if (symbol.IsTypeDefinition)
            {
                if (Typedefs.Contains(symbol))
                {
                    continue;
                }

                GenerateTypedef(symbol);
            }
            else if (symbol.IsTypeHeader)
            {
                var def = GetTypeDefinition(symbol);

                if (def != null)
                {
                    Typedefs.Add(def);
                }

                GenerateType(symbols, def);
            }
            else
            {
                throw new InvalidOperationException(symbol.ToString());
            }

            Writer.WriteLine();
        }

        return Writer.InnerWriter.ToString()!;
    }

    private void GenerateType(Symbol[] symbols, Symbol? def)
    {
        var header = symbols[0];

        var tag = def == null ? GetSafeName(header) : def.Tag == def.Name || def.HasFakeTag ? def.Name! : def.Tag!;

        tag = $"{SymbolGenerator.ToString((def ?? header).Type!.Value.Kind)} {tag}";

        Writer.WriteLine2($"{tag} ", $"// {def}".TrimEnd());

        Writer.WriteLine2("{", $"// {header}");

        Writer.Indent++;

        var members = symbols[1..^1];

        foreach (var member in members)
        {
            var type = GetMemberType(member);

            var mods = member.Type!.Value.Modifiers.ToArray();

            var ptrs = new string('*', mods.Count(s => s is SymbolTypeModifier.PTR));

            var name = member.Name;

            if (mods.Any(s => s is SymbolTypeModifier.FCN))
            {
                Writer.WriteLine2($"{type} ({ptrs}{name})();", $"// {member}");
            }
            else
            {
                var dims = string.Concat((member.Dimensions ?? []).Select(s => $"[{s}]"));

                var text = member.Class is SymbolStorageClass.FIELD
                    ? $"{type}{ptrs} {name}{dims} : {member.Size};"
                    : $"{type}{ptrs} {name}{dims};";

                Writer.WriteLine2(text, $"// {member}");
            }
        }

        Writer.Indent--;

        Writer.WriteLine2("};", $"// {symbols[^1]}");
    }

    private void GenerateTypedef(Symbol header)
    {
        if (header.Tag == null || header.Type!.Value.Modifiers.Any())
        {
            GenerateTypedefBasic(header);
        }
        else
        {
            GenerateTypedefComplex(header);
        }
    }

    private void GenerateTypedefBasic(Symbol def)
    {
        var tdef = SymbolGenerator.ToString(SymbolStorageClass.TPDEF);

        var type = def.Type!.Value;

        var kind = SymbolGenerator.ToString(type.Kind);

        var mods = type.Modifiers.ToArray();

        var ptrs = new string('*', mods.Count(s => s is SymbolTypeModifier.PTR));

        var dims = string.Concat((def.Dimensions ?? []).Select(s => $"[{s}]"));

        var name = def.Name;

        if (mods.Any(s => s is SymbolTypeModifier.FCN))
        {
            Writer.WriteLine2($"{tdef} {kind} ({ptrs}{name})();", $"// {def}");
        }
        else if (def.Tag == null)
        {
            Writer.WriteLine2($"{tdef} {kind}{ptrs} {name}{dims};", $"// {def}");
        }
        else
        {
            Writer.WriteLine2($"{tdef} {kind} {def.Tag}{ptrs} {def.Name};", $"// {def}");
        }
    }

    private void GenerateTypedefComplex(Symbol def)
    {
        if (def.Type!.Value.Modifiers.Any())
        {
            throw new ArgumentOutOfRangeException(nameof(def), def, null);
        }

        var type = GetType(def);

        GenerateType(type, def);
    }

    [SuppressMessage("ReSharper", "RedundantIfElseBlock")]
    private string GetMemberType(Symbol member)
    {
        var memberType = member.Type!.Value;

        var kind = SymbolGenerator.ToString(memberType.Kind);

        if (string.IsNullOrWhiteSpace(member.Tag))
        {
            // there are many typedefs for a type, e.g. unsigned short may be u_short or uid_t
            // since this can't really be solved, we default to the most likely, i.e. u_short
            // if not doing this, we'd end up with many members being uid_t, which is worse

            return TypedefsOverrides.TryGetValue(memberType.Kind, out var name) ? name : kind;
        }
        else
        {
            // this is slightly more complex, type name is found in a previous symbol

            return $"{kind} {GetMemberTypeName(member)}";
        }
    }

    private string GetMemberTypeName(Symbol member)
    {
        // easy case: in the tag, when there's one and it isn't fake
        // hard case: in a previous symbol whose name may be fake

        if (!member.HasFakeTag)
        {
            return member.Tag!;
        }

        var index = Array.IndexOf(Symbols, member);

        for (var i = index - 1; i >= 0; i--)
        {
            var symbol = Symbols[i];

            if (symbol.IsTypeHeader && symbol.Name == member.Tag)
            {
                return GetSafeName(symbol);
            }

            if (symbol.IsTypeDefinition && symbol.Tag == member.Tag)
            {
                return symbol.Name!;
            }
        }

        throw new InvalidOperationException();
    }

    private static string GetSafeName(Symbol symbol)
    {
        // most-effective way to make recycled fake names unique: use its position

        var name = symbol.Name ?? throw new ArgumentOutOfRangeException(nameof(symbol), symbol, null);

        if (symbol.HasFakeName)
        {
            name = $"_{name[1..]}_{symbol.Header.Position:x6}";
        }

        return name;
    }

    private Symbol[] GetType(Symbol def)
    {
        // associated type is generally right before typedef but not always...

        if (def.Tag == null || def.Type!.Value.Modifiers.Any())
        {
            throw new ArgumentOutOfRangeException(nameof(def), def, null);
        }

        if (!def.IsTypeDefinition)
        {
            throw new ArgumentOutOfRangeException(nameof(def), def, null);
        }

        var index1 = Array.FindIndex(SymbolsGroups, s => s[0] == def);

        if (index1 == -1)
        {
            throw new ArgumentOutOfRangeException(nameof(def), def, null);
        }

        var index2 = Array.FindLastIndex(SymbolsGroups, index1 - 1, s => s[0] is { IsTypeHeader: true } t && t.Name == def.Tag);

        if (index2 == -1)
        {
            throw new InvalidOperationException();
        }

        return SymbolsGroups[index2];
    }

    private Symbol? GetTypeDefinition(Symbol type)
    {
        // associated typedef is right after the type, but only when it has one

        if (!type.IsTypeHeader)
        {
            throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        var index = Array.FindIndex(SymbolsGroups, s => s[0] == type);

        if (index == -1)
        {
            throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        index++;

        if (index >= SymbolsGroups.Length)
        {
            return null;
        }

        var symbol = SymbolsGroups[index][0];

        if (symbol.IsTypeDefinition && symbol.Tag == type.Name && !symbol.Type!.Value.Modifiers.Any())
        {
            return symbol;
        }

        return null;
    }
}