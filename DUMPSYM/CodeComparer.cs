// #define LOG
// TODO variables and possibly others returning 0 shall be compared by their addresses
// TODO fakes shall be first (types, maybe others?)

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DUMPSYM;

public sealed class CodeComparer : Comparer<Code>
{
    public static CodeComparer Instance { get; } = new();

    public override int Compare(Code? x, Code? y)
    {
        Assert.IsNotNull(x);
        Assert.IsNotNull(y);

        var xSymbol = x.Symbols[0];
        var ySymbol = y.Symbols[0];

#if LOG
        Console.WriteLine($"xSymbol: {xSymbol}");
        Console.WriteLine($"ySymbol: {ySymbol}");
        Console.WriteLine();
#endif

        var xPriority = GetPriority(x);
        var yPriority = GetPriority(y);

        Assert.AreNotEqual(Priority.UNKNOWN, xPriority, xSymbol.ToString());
        Assert.AreNotEqual(Priority.UNKNOWN, yPriority, ySymbol.ToString());

        if (xPriority != yPriority)
        {
            return xPriority.CompareTo(yPriority);
        }

        return Compare(x, y, xPriority);
    }

    private static int Compare(Code x, Code y, Priority p)
        // Value Meaning
        // Less than zero x is less than y.
        // Zero x equals y.
        // Greater than zero x is greater than y.
    {
        if (p is Priority.STRUCT or Priority.UNION)
        {
            return CompareTypes(x, y);
        }

        if (p is Priority.EXTERNAL)
        {
            return 0;
        }

        if (p is Priority.TYPEDEF)
        {
            return 0;
        }

        if (p is Priority.FUNCTION)
        {
            return 0;
        }

        if (p is Priority.STATIC)
        {
            return 0;
        }

        if (p is Priority.NAME)
        {
            return 0;
        }

        throw new NotImplementedException($"\n{p}\n{x[0]}\n{y[0]}\n");
    }

    private static int CompareTypes(Code x, Code y)
    {
        var xSymbols = x[1..^1];
        var ySymbols = y[1..^1];

        var xHeader = (ISymbolDefinition)x[0];
        var yHeader = (ISymbolDefinition)y[0];

        if (ySymbols.OfType<ISymbolDefinition2>().Any(s => s.Tag == xHeader.Name))
        {
            return -1;
        }

        if (xSymbols.OfType<ISymbolDefinition2>().Any(s => s.Tag == yHeader.Name))
        {
            return +1;
        }

        return 0;
    }


    private static Priority GetPriority(Code code)
    {
        var symbol = code[0];

        return symbol switch
        {
            ISymbolDefinition def => def.Class switch
            {
                SymbolStorageClass.TPDEF  => Priority.TYPEDEF,
                SymbolStorageClass.EXT    => Priority.EXTERNAL,
                SymbolStorageClass.STRTAG => Priority.STRUCT,
                SymbolStorageClass.UNTAG  => Priority.UNION,
                SymbolStorageClass.STAT   => Priority.STATIC,
                _                         => Priority.UNKNOWN
            },
            ISymbolVariable  => Priority.NAME,
            ISymbolFileStart => Priority.FILE,
            ISymbolFunction  => Priority.FUNCTION,
            _                => Priority.UNKNOWN
        };
    }

    private enum Priority
    {
        TYPEDEF  = 0,
        STRUCT   = 1,
        UNION    = 2,
        EXTERNAL = 3,
        FUNCTION = 4,
        STATIC   = 5,
        NAME     = 6,
        FILE     = 7,
        UNKNOWN  = 8
    }
}