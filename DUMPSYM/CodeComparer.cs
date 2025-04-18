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

        if (false)
        {
            if (x.ToString().Contains("LoadFiles"))
            {
                var i = 0;
                Console.WriteLine(x);
                Console.WriteLine(y);
                Console.WriteLine();
            }

            if (y.ToString().Contains("LoadFiles"))
            {
                var i = 0;
                Console.WriteLine(x);
                Console.WriteLine(y);
                Console.WriteLine();
            }
        }

        var xIsType = x.IsType(out var xType);
        var yIsType = y.IsType(out var yType);

        var xIsTypedef = x.IsTypedef(out var xTypedef);
        var yIsTypedef = y.IsTypedef(out var yTypedef);

        if (xIsType)
        {
            if (yIsTypedef)
            {
                if (TypedefDependsOnType(yTypedef, xType))
                {
                    return -1;
                }
            }
        }

        if (yIsType)
        {
            if (xIsTypedef)
            {
                if (TypedefDependsOnType(xTypedef, yType))
                {
                    return +1;
                }
            }
        }

        if (xIsType && yIsType)
        {
            if (TypeDependsOnType(x, yType))
            {
                return -1;
            }

            if (TypeDependsOnType(y, xType))
            {
                return +1;
            }
        }

        return 1;
    }

    private static bool TypedefDependsOnType(ISymbolDefinition typedef, ISymbolDefinition type)
    {
        Console.WriteLine(typedef);
        Console.WriteLine(type);
        Console.WriteLine();
        if (type.Name is "LoadFiles")
        {
            var z = 0;
        }

        if (type.Name is "LoadFilesPtr")
        {
            var z = 0;
        }

        if (typedef.Name is "LoadFiles")
        {
            var z = 0;
        }

        if (typedef.Name is "LoadFilesPtr")
        {
            var z = 0;
        }

        return typedef is ISymbolDefinition2 d && d.Tag == type.Name;
    }

    private static bool TypeDependsOnType(Code source, ISymbolDefinition target)
    {
        return source.OfType<ISymbolDefinition2>().Any(s => s.Tag == target.Name);
    }
}