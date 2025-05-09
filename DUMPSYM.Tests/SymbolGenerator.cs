using System.Diagnostics.CodeAnalysis;

// ReSharper disable CommentTypo

namespace DUMPSYM.Tests;

public sealed class SymbolGenerator(SymbolFactory factory)
// BUG SpuStCallbackProc is typedef void (*SpuStCallbackProc)(unsigned long, long); in SDK but is Def class TPDEF type PTR FCN VOID size 0 name SpuStCallbackProc
{
    private SymbolFactory Factory { get; } = factory;

    private bool GenerateTypeEnabled { get; } = false;

    private bool GenerateTypeDefinitionEnabled { get; } = true;

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

        var s = string.Join(Environment.NewLine, dictionary);

        return s;
    }

    private string GenerateType(Symbol[] type)
    {
        var header = type[0];

        var value = $"TYPE = {header.Name!} -> {Factory.DistinctTypeName[type]} -> {Factory.DistinctTypeDefinitionMap[type]?.Name}";

        return value;
    }

    private void GenerateTypes(SortedDictionary<long, string> declarations)
    {
        foreach (var type in Factory.DistinctType)
        {
            var symbol = type[0];

            switch (symbol.Class)
            {
                case SymbolStorageClass.STRTAG:
                    break;
                case SymbolStorageClass.UNTAG:
                    break;
                default:
                    throw new NotImplementedException(symbol.Class.ToString());
            }

            var s = GenerateType(type);

            declarations.Add(symbol.Header.Position, s);
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