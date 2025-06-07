namespace DUMPSYM.Extensions;

public static class SymbolExtensions // TODO move
{
    #region Is*

    public static bool IsExternal(this ISymbol symbol)
    {
        return symbol is ISymbolDefinition { Class: SymbolStorageClass.EXT };
    }

    public static bool IsFileEnd(this ISymbol symbol)
    {
        return symbol is ISymbolFileEnd;
    }

    public static bool IsName(this ISymbol symbol)
    {
        return symbol is ISymbolVariable;
    }

    public static bool IsStatic(this ISymbol symbol)
    {
        return symbol is ISymbolDefinition { Class: SymbolStorageClass.STAT };
    }

    public static bool IsTypedef(this ISymbol symbol)
    {
        return symbol is ISymbolDefinition { Class: SymbolStorageClass.TPDEF };
    }

    #endregion

    #region Multiple

    public static bool IsFileHeader(this ISymbol symbol)
    {
        return symbol is ISymbolFileStart;
    }

    public static bool IsFileFooter(this ISymbol symbol)
    {
        return symbol is ISymbolFileEnd;
    }

    public static bool IsFunctionHeader(this ISymbol symbol)
    {
        return symbol is ISymbolFunction;
    }

    public static bool IsFunctionFooter(this ISymbol symbol)
    {
        return symbol is ISymbolFunctionEnd;
    }

    public static bool IsStructHeader(this ISymbol symbol)
    {
        return symbol is ISymbolDefinition { Class: SymbolStorageClass.STRTAG, Type.Kind: SymbolTypeKind.STRUCT };
    }

    public static bool IsUnionHeader(this ISymbol symbol)
    {
        return symbol is ISymbolDefinition { Class: SymbolStorageClass.UNTAG, Type.Kind: SymbolTypeKind.UNION };
    }

    [Obsolete("Use overload with type parameter.")]
    public static bool IsTypeFooter(this ISymbol symbol)
    {
        return symbol is ISymbolDefinition { Class: SymbolStorageClass.EOS, Type.Kind: SymbolTypeKind.NULL, Name: ".eos" };
    }

    #endregion
}