using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;

namespace DUMPSYM.Tests;

public sealed class HeaderGenerator
// TODO 'typedef struct _GsCOORDINATE { ... } GsCOORDINATE' stays as _GsCOORDINATE or IDA generates fake type -> best done on IDA output
{
    private const bool JumpLines = false;

    private HashSet<Symbol> Typedefs { get; } = [];

    public IndentedTextWriter Writer { get; } = new(new StringWriter());

    public int Remove(List<Symbol[]> split, Func<Symbol, bool> predicate)
    {
        return split.RemoveAll(s => predicate(s[0]));
    }

    public void Generate(Symbol[][] filtered, List<Symbol> original)
    {
        var originals = original.ToArray();

        foreach (var (index, array) in filtered.Index())
        {
            var header = array[0];

            if (header.IsTypeDefinition)
            {
                if (!Typedefs.Contains(header))
                {
                    GenerateTypedef(filtered, header, originals);
                }
            }
            else if (header.IsTypeHeader)
            {
                var nextOffset = index + 1;

                if (nextOffset >= 0 && nextOffset < filtered.Length)
                {
                    var nextSymbol = filtered[nextOffset]; // TODO sucks, need better mechanism

                    var nextHeader = nextSymbol[0];

                    if (nextHeader.IsTypeDefinition && nextHeader.Tag == header.Name && !nextHeader.Type!.Value.Modifiers.Any())
                    {
                        Typedefs.Add(nextHeader);

                        GenerateType(nextHeader, array, originals);
                    }
                    else // LoadFiles
                    {
                        GenerateType(null, array, originals); // TODO should be triggered by compiler generated struct
                    }
                }
                else
                {
                    GenerateType(null, array, originals); // TODO should be triggered by compiler generated struct
                }
            }
            else
            {
                throw new InvalidOperationException(header.ToString());
            }

            if (JumpLines)
            {
                Writer.WriteLine();
            }
        }
    }

    private void GenerateType(Symbol? definition, Symbol[] type, Symbol[] everything)
    {
        var header = type[0];

        string typeName;

        if (definition == null) // TODO should be triggered by compiler-generated struct
        {
            typeName = $"{SymbolGenerator.ToString(header.Type!.Value.Kind)} {GetSafeName(header)}";
        }
        else
        {
            string name;

            if (definition.Tag != definition.Name && !definition.HasFakeTag)
            {
                name = definition.Tag!;
            }
            else
            {
                name = definition.Name!;
            }

            typeName = $"{SymbolGenerator.ToString(definition.Type!.Value.Kind)} {name}";
        }

        Writer.WriteLine2($"{typeName} ", $"// {definition}".TrimEnd());

        Writer.WriteLine2("{", $"// {header}");

        Writer.Indent++;

        var members = type[1..^1];

        foreach (var member in members)
        {
            var kind = GetMemberString(member, everything);

            var memberType = member.Type!.Value;

            var modifiers = memberType.Modifiers.ToArray();

            var pointers = new string('*', modifiers.Count(s => s is SymbolTypeModifier.PTR));

            if (modifiers.Any(s => s is SymbolTypeModifier.FCN))
            {
                Writer.WriteLine2($"{kind} ({pointers}{member.Name})();", $"// {member}");
            }
            else
            {
                var dimensions = string.Concat((member.Dimensions ?? []).Select(s => $"[{s}]"));

                Writer.WriteLine2($"{kind}{pointers} {member.Name}{dimensions};", $"// {member}");
            }
        }

        Writer.Indent--;

        Writer.WriteLine2("};", $"// {type[^1]}");
    }

    private void GenerateTypedef(Symbol[][] filtered, Symbol header, Symbol[] originals)
    {
        if (header.Tag == null)
        {
            GenerateTypedefBasic(header);
        }
        else
        {
            GenerateTypedefComplex(header, filtered, originals);
        }
    }

    [SuppressMessage("ReSharper", "ConvertIfStatementToConditionalTernaryExpression")]
    private void GenerateTypedefBasic(Symbol def)
    {
        var type = def.Type!.Value;

        var modifiers = type.Modifiers.ToArray();

        var pointers = new string('*', modifiers.Count(s => s is SymbolTypeModifier.PTR));

        var dimensions = string.Concat((def.Dimensions ?? []).Select(s => $"[{s}]"));

        var typedef = SymbolGenerator.ToString(SymbolStorageClass.TPDEF);

        var kind = SymbolGenerator.ToString(type.Kind);

        if (modifiers.Any(s => s is SymbolTypeModifier.FCN))
        {
            Writer.WriteLine2($"{typedef} {kind} ({pointers}{def.Name})();", $"// {def}");
        }
        else
        {
            Writer.WriteLine2($"{typedef} {kind}{pointers} {def.Name}{dimensions};", $"// {def}");
        }
    }

    private void GenerateTypedefComplex(Symbol def, Symbol[][] symbols, Symbol[] everything)
    {
        var index = Array.FindIndex(symbols, s => s[0] == def);

        if (index is -1)
        {
            throw new InvalidOperationException();
        }

        var type = def.Type!.Value;

        var modifiers = type.Modifiers.ToArray();

        if (modifiers.Any())
        {
            var s1 = SymbolGenerator.ToString(SymbolStorageClass.TPDEF);
            var s2 = SymbolGenerator.ToString(type.Kind);
            var s3 = new string('*', modifiers.Count(s => s is SymbolTypeModifier.PTR));

            Writer.WriteLine2($"{s1} {s2} {def.Tag}{s3} {def.Name};", $"// {def}");
        }
        else
        {
            for (var i = index - 1; i >= 0; i--)
            {
                var symbol = symbols[i];

                var header = symbol[0];

                if (header.IsTypeHeader && header.Name == def.Tag)
                {
                    GenerateType(def, symbol, everything);
                    break;
                }
            }
        }
    }

    private string GetMemberString(Symbol member, Symbol[] symbols)
    {
        var memberType = member.Type!.Value;

        var find = Array.Find(symbols, s => s.IsTypeDefinition && s.Type!.Value.Kind == memberType.Kind && !s.Type!.Value.Modifiers.Any());

        var kind = SymbolGenerator.ToString(memberType.Kind);

        string output;

        if (string.IsNullOrWhiteSpace(member.Tag)) //member.Tag != null)
        {
            output = find?.Name ?? kind;
        }
        else
        {
            if (member.HasFakeTag)
            {
                var index = Array.IndexOf(symbols, member);

                var name = default(string);

                for (var i = index - 1; i >= 0; i--)
                {
                    var symbol = symbols[i];

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
        }

        if (memberType.Kind is SymbolTypeKind.STRUCT or SymbolTypeKind.UNION)
        {
            output = $"{kind} {output}";
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
}