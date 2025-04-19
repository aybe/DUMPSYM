// ReSharper disable RedundantIfElseBlock

using Microsoft.VisualStudio.TestTools.UnitTesting;

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

        var xIsType = x.IsType(out var xType);
        var yIsType = y.IsType(out var yType);

        var xIsTypedef = x.IsTypedef(out var xTypedef);
        var yIsTypedef = y.IsTypedef(out var yTypedef);

        if (xIsType && yIsType)
        {
            if (xType.Name.Contains("Sprite"))
            {
                var z = 0;
            }

            if (yType.Name.Contains("Sprite"))
            {
                var z = 0;
            }

            if (xType.Name.Contains("Sprite") && yType.Name.Contains("Sprite"))
            {
                var z = 0;
            }

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

            if (TypedefDependsOnType(yTypedef, xType))
            {
                return -1; // ok // TODO never reached on some tests
            }
            else
            {
                var xFake = SymbolRegistry.HasFakeName(xType.Name);
                var yFake = SymbolRegistry.HasFakeName(yDef2.Tag);

                if (yFake && !xFake)
                {
                    return +1; // ok
                }

                return -1; // ok
            }
        }

        // BUG SpriteData requires Sprite
        // BUG game typedefs shall be before game structs

        if (xIsTypedef && yIsType)
        {
            if (xTypedef is not ISymbolDefinition2 xDef2)
            {
                return -1; // ok
            }

            if (TypedefDependsOnType(xTypedef, yType))
            {
                return +1; // ok // TODO never reached on some tests
            }
            else
            {
                var xFake = SymbolRegistry.HasFakeName(xDef2.Tag);
                var yFake = SymbolRegistry.HasFakeName(yType.Name);

                if (xFake && !yFake)
                {
                    return -1; // ok
                }

                return +1; // ok
            }
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

    private static bool TypeDependsOnType(Code source, ISymbolDefinition target)
    {
        if (source.ToString().Contains("Sprite"))
        {
            var zero = 0;
        }

        if (target.Name.Contains("Sprite"))
        {
            var zero = 0;
        }

        if (source.ToString().Contains("Sprite") && target.ToString().Contains("SpriteData"))
        {
            var zero = 0; // BUG should be reached
        }

        if (source.ToString().Contains("SpriteData") && target.ToString().Contains("Sprite"))
        {
            var zero = 0; // BUG should be reached
        }

        return source.OfType<ISymbolDefinition2>().Any(s => s.Tag == target.Name);
    }
}