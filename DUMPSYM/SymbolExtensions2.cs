using System.Diagnostics.CodeAnalysis;

namespace DUMPSYM;

public static class SymbolExtensions2
{
    public static bool IsDef1(this Symbol symbol, [MaybeNullWhen(false)] out ISymbolDefinition result)
    {
        result = null;

        if (symbol.Record is not ISymbolDefinition2 && symbol.Record is ISymbolDefinition d)
        {
            result = d;
        }

        return result != null;
    }

    public static bool IsDef2(this Symbol symbol, [MaybeNullWhen(false)] out ISymbolDefinition2 result)
    {
        result = null;

        if (symbol.Record is ISymbolDefinition2 d)
        {
            result = d;
        }

        return result != null;
    }

    public static bool IsTypedef(this Symbol symbol)
    {
        return symbol.IsTypedef1(out _) || symbol.IsTypedef2(out _);
    }

    public static bool IsTypedef1(this Symbol symbol, [MaybeNullWhen(false)] out ISymbolDefinition result)
    {
        result = null;

        if (IsDef1(symbol, out var d) && d.Class is SymbolStorageClass.TPDEF)
        {
            result = d;
        }

        return result != null;
    }

    public static bool IsTypedef2(this Symbol symbol, [MaybeNullWhen(false)] out ISymbolDefinition2 result)
    {
        result = null;

        if (IsDef2(symbol, out var d) && d.Class is SymbolStorageClass.TPDEF)
        {
            result = d;
        }

        return result != null;
    }

    public static bool IsType(this Symbol symbol, [MaybeNullWhen(false)] out ISymbolDefinition result)
    {
        result = null;

        if (IsDef1(symbol, out var d) && d.Class is SymbolStorageClass.STRTAG or SymbolStorageClass.UNTAG)
        {
            result = d;
        }

        return result != null;
    }

    public static bool IsTypeEnd(this Symbol symbol, [MaybeNullWhen(false)] out ISymbolDefinition2 result)
    {
        result = null;

        if (IsDef2(symbol, out var d) && d.Class is SymbolStorageClass.EOS)
        {
            result = d;
        }

        return result != null;
    }
}