using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable IdentifierTypo

namespace DUMPSYM.Generators;

public sealed class IdaHeaderGenerator(IdaGenerator generator) : IDisposable
// generating IDA-friendly code requires to not use typedef struct/union at all
// else, IDA will rename every type with a fake name and add an extra typedef
{
    private IdaGenerator Generator { get; } = generator;

    private IndentedTextWriter Writer { get; } = new(new StringWriter());

    public void Dispose()
    {
        Writer.Dispose();
    }

    public string Generate()
    {
        // as a type may generate its own typedef, keep track of them

        foreach (var symbols in Generator.SymbolsGroups)
        {
            var symbol = symbols[0];

            if (symbol.IsTypeDefinition)
            {
                if (Generator.Typedefs.Contains(symbol))
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
                    Generator.Typedefs.Add(def);
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

            return Generator.TypedefsOverrides.GetValueOrDefault(memberType.Kind, kind);
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

        var index = Array.IndexOf(Generator.Symbols, member);

        for (var i = index - 1; i >= 0; i--)
        {
            var symbol = Generator.Symbols[i];

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

        var index1 = Array.FindIndex(Generator.SymbolsGroups, s => s[0] == def);

        if (index1 == -1)
        {
            throw new ArgumentOutOfRangeException(nameof(def), def, null);
        }

        var index2 = Array.FindLastIndex(Generator.SymbolsGroups, index1 - 1, s => s[0] is { IsTypeHeader: true } t && t.Name == def.Tag);

        if (index2 == -1)
        {
            throw new InvalidOperationException();
        }

        return Generator.SymbolsGroups[index2];
    }

    private Symbol? GetTypeDefinition(Symbol type)
    {
        // associated typedef is right after the type, but only when it has one

        if (!type.IsTypeHeader)
        {
            throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        var index = Array.FindIndex(Generator.SymbolsGroups, s => s[0] == type);

        if (index == -1)
        {
            throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        index++;

        if (index >= Generator.SymbolsGroups.Length)
        {
            return null;
        }

        var symbol = Generator.SymbolsGroups[index][0];

        if (symbol.IsTypeDefinition && symbol.Tag == type.Name && !symbol.Type!.Value.Modifiers.Any())
        {
            return symbol;
        }

        return null;
    }
}