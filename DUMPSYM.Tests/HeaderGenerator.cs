using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable IdentifierTypo

// ReSharper disable CommentTypo

namespace DUMPSYM.Tests;

public sealed class HeaderGenerator : IDisposable
// generator produces IDA-friendly code with a simpler C syntax,
// i.e. typedef struct symbols are stripped out of the typedef 
// else IDA produces fake types which isn't friendly at all
{
    public HeaderGenerator(List<Symbol> symbols, HeaderGeneratorOptions options)
    {
        // cleanup symbols first to produce cleanest possible output
        // functions are a trap as they contain types and typedefs
        // others are useless and will be done in an IDA script

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

        // most of the types and typedefs are duplicates, except for fake types
        // compiler generates them and reuse the same names making it tricky
        // with a special comparer, we can differentiate these from others

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
        // there tends to be as many duplicate symbols as there are files

        foreach (var name in typedefs)
        {
            var array = symbols.Where(s => s.IsTypeDefinition && s.Name == name).ToArray();

            var count = symbols.RemoveAll(array.Contains);

            Console.WriteLine($"Removed {count} instances of '{name}'");
        }
    }

    private void CleanupTypedefsUnsigned(List<Symbol> symbols)
    {
        // UNIX typedefs may exist but as we use SDK unsigned typedefs they're useless

        CleanupTypedefs(symbols, ["ushort", "uint", "ulong"]);

        foreach (var typedef in TypedefsOverrides.Values)
        {
            Console.WriteLine($"Added instance of '{typedef}'");
        }

        // insert SDK unsigned typedefs after first file as splitting is done by file

        var index = symbols.FindIndex(s => s.IsFileEnd);

        var array = TypedefsOverrides
            .Select(s => new Symbol(new SymbolHeader { Type = 0x94 }, new SymbolRecordDef(SymbolStorageClass.TPDEF, new SymbolType(s.Key), 0, s.Value)))
            .ToArray();

        symbols.InsertRange(index + 1, array);
    }

    public string Generate()
    {
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
                if (TryGetDefinition(symbol, out var def))
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

    private void GenerateType(Symbol[] type, Symbol? def)
    {
        var header = type[0];

        var tag = def == null ? GetSafeName(header) : def.Tag == def.Name || def.HasFakeTag ? def.Name! : def.Tag!;

        tag = $"{SymbolGenerator.ToString((def ?? header).Type!.Value.Kind)} {tag}";

        Writer.WriteLine2($"{tag} ", $"// {def}".TrimEnd());

        Writer.WriteLine2("{", $"// {header}");

        Writer.Indent++;

        var members = type[1..^1];

        foreach (var member in members)
        {
            var kind = GetMemberString(member);

            var mods = member.Type!.Value.Modifiers.ToArray();

            var ptrs = new string('*', mods.Count(s => s is SymbolTypeModifier.PTR));

            var name = member.Name;

            if (mods.Any(s => s is SymbolTypeModifier.FCN))
            {
                Writer.WriteLine2($"{kind} ({ptrs}{name})();", $"// {member}");
            }
            else
            {
                var dims = string.Concat((member.Dimensions ?? []).Select(s => $"[{s}]"));

                var text = member.Class is SymbolStorageClass.FIELD
                    ? $"{kind}{ptrs} {name}{dims} : {member.Size};"
                    : $"{kind}{ptrs} {name}{dims};";

                Writer.WriteLine2(text, $"// {member}");
            }
        }

        Writer.Indent--;

        Writer.WriteLine2("};", $"// {type[^1]}");
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

    [SuppressMessage("ReSharper", "ConvertIfStatementToConditionalTernaryExpression")]
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

    private string GetMemberString(Symbol member)
    {
        var memberType = member.Type!.Value;

        var index = Array.IndexOf(Symbols, member);

        var kind = SymbolGenerator.ToString(memberType.Kind);

        string output;

        if (string.IsNullOrWhiteSpace(member.Tag))
        {
            // there are many typedefs for a type, e.g. unsigned short may be u_short or uid_t
            // since this can't really be solved, we default to the most likely, i.e. u_short
            // if not doing this, we'd end up with many members being uid_t, which is worse

            output = TypedefsOverrides.TryGetValue(memberType.Kind, out var name) ? name : kind;
        }
        else
        {
            if (member.HasFakeTag)
            {
                var name = default(string);

                for (var i = index - 1; i >= 0; i--)
                {
                    var symbol = Symbols[i];

                    if (symbol.IsTypeHeader && symbol.Name == member.Tag)
                    {
                        name ??= GetSafeName(symbol);
                        break;
                    }

                    if (symbol.IsTypeDefinition && symbol.Tag == member.Tag)
                    {
                        var aggregate = (member.Dimensions ?? [1u]).Aggregate(1u, (s, t) => s * t);
                        var memberSize = member.Size!.Value / aggregate;
                        Assert.AreEqual(memberSize, symbol.Size);
                        name = symbol.Name;
                        break;
                    }
                }

                output = name ?? throw new InvalidOperationException(member.ToString());
            }
            else
            {
                output = member.Tag;
            }

            if (memberType.Kind is SymbolTypeKind.STRUCT or SymbolTypeKind.UNION)
            {
                output = $"{kind} {output}";
            }
        }

        return output;
    }

    private static string GetSafeName(Symbol symbol)
    {
        var name = symbol.Name ?? throw new ArgumentOutOfRangeException(nameof(symbol), symbol, null);

        if (symbol.HasFakeName)
        {
            name = $"_{name[1..]}_{symbol.Header.Position:x6}";
        }

        return name;
    }

    private Symbol[] GetType(Symbol def)
    {
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

    private bool TryGetDefinition(Symbol type, [MaybeNullWhen(false)] out Symbol result)
    {
        result = null;

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
            return false;
        }

        var symbol = SymbolsGroups[index][0];

        if (symbol.IsTypeDefinition && symbol.Tag == type.Name && !symbol.Type!.Value.Modifiers.Any())
        {
            result = symbol;
        }

        return result != null;
    }
}