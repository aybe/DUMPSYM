using System.Runtime.CompilerServices;

namespace DUMPSYM;

public static class TypedefUtility
{
    public static string Parse(ISymbolDefinition def)
    {
        if (def is ISymbolDefinition2 def2)
        {
            return ParseComplex(def2);
        }

        return ParseSimple(def);
    }

    private static string ParseSimple(ISymbolDefinition def)
    {
        var handler = new DefaultInterpolatedStringHandler();

        handler.AppendLiteral("typedef");

        handler.AppendLiteral(" ");

        var type = def.Type;

        handler.AppendLiteral(GetKindString(type.Kind));

        var modifiers = type.Modifiers.ToArray();

        var pointers = modifiers.Count(s => s == SymbolTypeModifier.PTR);

        if (modifiers.All(s => s != SymbolTypeModifier.FCN))
        {
            for (var i = 0; i < pointers; i++)
            {
                handler.AppendLiteral("*");
            }
        }

        handler.AppendLiteral(" ");

        var function = modifiers.Any(s => s == SymbolTypeModifier.FCN);

        if (function)
        {
            handler.AppendLiteral("(");

            for (var i = 0; i < pointers; i++)
            {
                handler.AppendLiteral("*");
            }
        }

        handler.AppendLiteral(def.Name);

        if (function)
        {
            handler.AppendLiteral(")()");
        }

        handler.AppendLiteral(";");

        var result = handler.ToStringAndClear();

        return result;
    }

    private static string ParseComplex(ISymbolDefinition2 def)
    {
        var handler = new DefaultInterpolatedStringHandler();

        if (def.Type.Kind == SymbolTypeKind.STRUCT)
        {
            if (def.Type.Modifiers.Contains(SymbolTypeModifier.PTR))
            {
                handler.AppendLiteral("typedef");
                handler.AppendLiteral(" ");
                handler.AppendLiteral("struct");
                handler.AppendLiteral(" ");
                handler.AppendLiteral(def.Tag);
                handler.AppendLiteral("*");
                handler.AppendLiteral(" ");
                handler.AppendLiteral(def.Name);
                handler.AppendLiteral(";");
            }
        }

        Console.WriteLine($"{def}\t\t\t{handler.ToStringAndClear()}");

        return handler.ToStringAndClear();
    }

    private static string GetKindString(SymbolTypeKind kind)
    {
        return kind switch
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
            SymbolTypeKind.MOE    => "enum member",
            SymbolTypeKind.UCHAR  => "unsigned char",
            SymbolTypeKind.USHORT => "unsigned short",
            SymbolTypeKind.UINT   => "unsigned int",
            SymbolTypeKind.ULONG  => "unsigned long",
            _                     => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
    }
}