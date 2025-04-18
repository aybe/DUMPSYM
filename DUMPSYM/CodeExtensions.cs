namespace DUMPSYM;

public static class CodeExtensions
{
    private static bool Is<T>(this Code code, Func<ISymbol, T> selector, Func<T, bool> predicate, out T result)
    {
        result = default!;

        var value = selector(code[0]);

        if (predicate(value))
        {
            result = value;
        }

        return result != null;
    }

    public static bool IsType(this Code code, out ISymbolDefinition result)
    {
        return code.Is(s => s as ISymbolDefinition, s => s?.Class is SymbolStorageClass.STRTAG or SymbolStorageClass.UNTAG, out result!);
    }

    public static bool IsTypedef(this Code code, out ISymbolDefinition result)
    {
        return code.Is(s => s as ISymbolDefinition, s => s?.Class is SymbolStorageClass.TPDEF, out result!);
    }
}