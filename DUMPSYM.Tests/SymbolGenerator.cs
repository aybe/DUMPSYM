using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using DUMPSYM.Extensions;

// ReSharper disable StringLiteralTypo

// ReSharper disable CommentTypo

namespace DUMPSYM.Tests;

public sealed class SymbolGenerator(SymbolFactory factory)
// BUG SpuStCallbackProc is typedef void (*SpuStCallbackProc)(unsigned long, long); in SDK but is Def class TPDEF type PTR FCN VOID size 0 name SpuStCallbackProc

// BUG shall the types of members of types without any typedef shall be generated using no typedef?
// e.g. EvCB, ULONG -> unsigned long
// but problem, how to differentiate these types? whether it's fake or not?

// TODO NCB hint https://github.com/joncampbell123/windows_sdk_collection/blob/main/win/3.1wfw/mswin/include/NCB.H
// TODO NCB hint https://github.com/turican0/remc2/blob/development/remc2/sub_main.h#L415

// BUG Camera ChaseCamera; /* case 1 */ // 00bdd9: $0000003c 96 Def2 class MOS type STRUCT size 60 dims 0 tag Camera name ChaseCamera
// BUG void Function; /* case 2 */ // 00f706: $00000010 94 Def class MOS type PTR FCN VOID size 0 name Function

// TODO missing typedefs: physadr
{
    private SymbolFactory Factory { get; } = factory;

    private bool GenerateTypeEnabled { get; } = true;

    private bool GenerateTypeDefinitionEnabled { get; } = false;

    public override string ToString()
    {
        var dictionary = new SortedDictionary<long, string>();

        if (GenerateTypeEnabled)
        {
            GenerateTypes(dictionary);
        }

        if (GenerateTypeDefinitionEnabled)
        {
            GenerateTypeDefinitions(dictionary);
        }

        var s = string.Join(Environment.NewLine, dictionary.Values);

        Console.WriteLine(dictionary.Count);

        return s;
    }

    [SuppressMessage("ReSharper", "RedundantIfElseBlock")]
    [SuppressMessage("ReSharper", "ConvertIfStatementToConditionalTernaryExpression")]
    private string? GenerateType(Symbol[] type)
        // TODO there can be a single method that just prepends typedef
    {
        using var writer = new IndentedTextWriter(new StringWriter());

        var def = Factory.DistinctTypeDefinitionMap[type];

        var header = type[0];

        var klass = ToString(header.Class!.Value);

        var name = Factory.DistinctTypeName[type];

        if (def == null)
        {
            writer.WriteLine2($"{klass} {name} {{", $"// {header}");
        }
        else
        {
            if (def.HasFakeTag)
            {
                writer.WriteLine2($"{ToString(SymbolStorageClass.TPDEF)} {klass} {{", $"// {def}");
            }
            else
            {
                writer.WriteLine2($"{ToString(SymbolStorageClass.TPDEF)} {klass} {def.Tag} {{", $"// {def}");
            }

            writer.WriteLine2(null, $"// {header}");
        }

        using (writer.GetIndentScope())
        {
            var members = type[1..^1];

            foreach (var member in members)
            {
                writer.WriteLine2($"{GetMemberString(member, header)}", $"// {member}");
            }
        }

        var footer = type[^1];

        if (def == null)
        {
            writer.WriteLine2("};", $"// {footer}");
        }
        else
        {
            writer.WriteLine2($"}} {name};", $"// {footer}");
        }

        return writer.InnerWriter.ToString();
    }

    [SuppressMessage("ReSharper", "RedundantIfElseBlock")]
    [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
    private string? GetMemberString(Symbol member, Symbol parent)
        // TODO figure out which of C primitive or typedef to use for type
        // TODO this should be able to work for EXT, TPDEF too
    {
        var name = member.Name;
        var type = member.Type!.Value;

        var modifiers = type.Modifiers.ToArray();

        var pointers = new string('*', modifiers.Count(s => s is SymbolTypeModifier.PTR));

        var dimensions = string.Concat((member.Dimensions ?? []).Select(s => $"[{s}]"));

        var field = member.Class!.Value is SymbolStorageClass.FIELD ? $" : {member.Size!.Value}" : null;

        var fcn = modifiers.Any(s => s is SymbolTypeModifier.FCN);

        if (fcn) // always have PTR
        {
            Assert.AreEqual(1, modifiers.Count(s => s is SymbolTypeModifier.FCN), member.ToString());
            Assert.AreEqual(0, modifiers.Count(s => s is SymbolTypeModifier.ARY), member.ToString());
        }

        var typeName = GetTypeName(member, parent);

        if (fcn)
        {
            return $"{typeName} ({pointers}{name})();";
        }
        else
        {
            return $"{typeName}{pointers} {name}{dimensions}{field};";
        }
    }

    private Symbol? GetTypeDefinition(Symbol member, Func<Symbol, bool> predicate)
    {
        var where1 = Factory.DistinctTypeDefinition.Where(predicate);

        var where2 = where1.Where(s => s.Header.Position < member.Header.Position);

        var symbol = where2.FirstOrDefault();

        return symbol;
    }

    [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
    private static string GetTypeName(Symbol symbol, bool typedef = false)
    {
        if (typedef)
        {
            return symbol.Name!;
        }

        return ToString(symbol.Type!.Value.Kind);
    }

    private void GenerateTypes(SortedDictionary<long, string> declarations)
    {
        foreach (var type in Factory.DistinctType.OrderBy(s => s[0].Header.Position)) // TODO delete
        {
            var value = GenerateType(type);

            if (value != null)
            {
                declarations.Add(type[0].Header.Position, value);
            }
        }
    }

    [SuppressMessage("ReSharper", "RedundantIfElseBlock")]
    [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
    private static string? GenerateTypeDefinition(Symbol symbol)
    {
        var tag = symbol.Tag;

        var type = symbol.Type!.Value;

        var klass = ToString(symbol.Class!.Value);

        var modifiers = type.Modifiers.ToArray();

        var pointers = new string('*', modifiers.Count(s => s is SymbolTypeModifier.PTR));

        if (tag == null)
        {
            var kind = ToString(type.Kind);

            if (modifiers.Any(s => s is SymbolTypeModifier.FCN))
            {
                return $"{klass} {kind} ({pointers}{symbol.Name})(void);";
            }
            else
            {
                return $"{klass} {kind}{pointers} {symbol.Name};";
            }
        }
        else
        {
            if (modifiers.Length != 0)
            {
                return $"{klass} {tag}{pointers} {symbol.Name};";
            }
            else
            {
                return null; // typedef struct X { ... } Y;
            }
        }
    }

    private void GenerateTypeDefinitions(SortedDictionary<long, string> dictionary)
    {
        foreach (var symbol in Factory.DistinctTypeDefinition)
        {
            var value = GenerateTypeDefinition(symbol);

            if (value != null)
            {
                dictionary.Add(symbol.Header.Position, value);
            }
        }
    }

    [SuppressMessage("ReSharper", "RedundantIfElseBlock")]
    [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
    private string GetTypeName(Symbol member, Symbol parent) // TODO comment
    {
        var tag = member.Tag;

        var type = member.Type!.Value;

        if (string.IsNullOrWhiteSpace(tag))
        {
            var symbol1 = GetTypeDefinition(member, s => s.Type!.Value == type);

            if (symbol1 != null)
            {
                return GetTypeName(symbol1); // identical
            }

            var symbol2 = GetTypeDefinition(member, s => s.Type!.Value.Kind == type.Kind);

            if (symbol2 != null)
            {
                return GetTypeName(symbol2); // partial
            }
            else
            {
                return GetTypeName(member); // manual
            }
        }
        else
        {
            if (Factory.DistinctTypeName.Values.Any(s => s == tag)) // TODO reverse map
            {
                return tag;
            }
            else
            {
                var a = Factory.DistinctTypeName;
                var b = a.Where(s => s.Key[0].Header.Position < parent.Header.Position);
                var c = b.Where(s => s.Key[0].Name == tag);
                var d = c.LastOrDefault().Value;

                if (d != null)
                {
                    return d;
                }
                else
                {
                    return $"{ToString(type.Kind)} {tag}";
                }
            }
        }
    }

    private static string ToString(SymbolStorageClass value)
    {
        var s = value switch
        {
            SymbolStorageClass.AUTO    => null,
            SymbolStorageClass.EXT     => null,
            SymbolStorageClass.STAT    => null,
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

    private static string ToString(SymbolTypeKind value)
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