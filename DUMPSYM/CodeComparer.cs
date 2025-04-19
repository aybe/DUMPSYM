using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable GrammarMistakeInComment

namespace DUMPSYM;

public sealed class CodeComparer : Comparer<Code>
{
    public static CodeComparer Instance { get; } = new();

    public int Count { get; set; }

    public override int Compare(Code? x, Code? y)
    {
        Count++;

        ArgumentNullException.ThrowIfNull(x);
        ArgumentNullException.ThrowIfNull(y);
        /*
         * RULES
         *
         * type
         *      can be fake
         *          must be before real type and real typedef
         *      can be real
         *          must be after fake type and before its eventual typedef
         *      can be dependent on type
         *          must be after that type
         *      can be dependent on typedef
         *          must be after that typedef
         *
         * typedef
         *      can map a fake type
         *          must be after that fake type
         *      can map a real type
         *          must be after that real type
         *      can be dependent on real type
         *          must be after that real type
         */
        var xIsType = x.IsType(out var xType);
        var yIsType = y.IsType(out var yType);

        var xIsTypedef = x.IsTypedef(out var xTypedef);
        var yIsTypedef = y.IsTypedef(out var yTypedef);

        if (xIsType && yIsType)
        {
            var xFake = SymbolRegistry.HasFakeName(xType.Name);
            var yFake = SymbolRegistry.HasFakeName(yType.Name);

            if (xFake)
            {
                return yFake ? 0 : -1;
            }

            if (yFake)
            {
                return xFake ? 0 : +1;
            }

            Assert.AreEqual(xFake, yFake);

            if (TypeDependsOnType(x, yType))
            {
                return +1;
            }

            if (TypeDependsOnType(y, xType))
            {
                return -1;
            }

            // no dependency between types

            if (xFake) // TODO never reached, delete
            {
                return string.Compare(xType.Name, yType.Name, StringComparison.Ordinal);
            }
        }

        if (xIsTypedef && yIsTypedef)
        {
            var x2 = xTypedef is ISymbolDefinition2;
            var y2 = yTypedef is ISymbolDefinition2;

            return x2.CompareTo(y2); // TODO looks much better with it
        }

        if (xIsType && yIsTypedef)
        {
            if (yTypedef is not ISymbolDefinition2 yDef2)
            {
                return +1; // ok
            }

            var a = TypeDependsOnTypedef(x, yTypedef);
            var b = TypedefDependsOnType(yTypedef, xType);

            if (a && b)
            {
                return +1; // kkkkkkk
            }

            if (a)
            {
                return -1;
            }

            if (b)
            {
                return -1; // ok // TODO never reached on some tests
            }

            var xFake = SymbolRegistry.HasFakeName(xType.Name);
            var yFake = SymbolRegistry.HasFakeName(yDef2.Tag);

            if (yFake && !xFake)
            {
                return +1; // ok
            }

            return -1; // ok
        }

        // BUG SpriteData requires Sprite
        // BUG game typedefs shall be before game structs

        if (xIsTypedef && yIsType)
        {
            if (xTypedef is not ISymbolDefinition2 xDef2)
            {
                return -1; // ok
            }

            var a = TypeDependsOnTypedef(y, xTypedef);

            var b = TypedefDependsOnType(xTypedef, yType);

            if (a && b)
            {
                return +1;
            }

            if (a)
            {
                return -1;
            }

            if (b)
            {
                return +1; // ok // TODO never reached on some tests
            }

            var xFake = SymbolRegistry.HasFakeName(xDef2.Tag);
            var yFake = SymbolRegistry.HasFakeName(yType.Name);

            if (xFake && !yFake)
            {
                return -1; // ok
            }

            return +1; // ok
        }

        return 0;
    }

    private static bool TypedefDependsOnType(ISymbolDefinition typedef, ISymbolDefinition type)
    {
        if (false)
        {
            Console.WriteLine(typedef);
            Console.WriteLine(type);
            Console.WriteLine();
        }

        return typedef is ISymbolDefinition2 d && d.Tag == type.Name;
    }

    private static bool TypeDependsOnTypedef(Code type, ISymbolDefinition typedef)
    {
        var b = type.OfType<ISymbolDefinition2>().Any(s => s.Tag == ((ISymbolDefinition2)typedef).Tag);

        if (type.ToString().EndsWith("Def class STRTAG type STRUCT size 1803672 name Editor") &&
            typedef.ToString()!.Contains("Def2 class MOS type ARY STRUCT size 32 dims 1 4 tag Coord3D name RecordPosition"))
        {
            Assert.IsTrue(b); // TODO delete
        }

        return b;
    }

    private static bool TypeDependsOnType(Code source, ISymbolDefinition target)
    {
        return source.OfType<ISymbolDefinition2>().Any(s => s.Tag == target.Name);
    }
}