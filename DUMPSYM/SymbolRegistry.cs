namespace DUMPSYM;

public sealed class SymbolRegistry
{
    public required List<ISymbol> Externals { get; init; }

    public required List<List<ISymbol>> Files { get; init; }

    public required List<List<ISymbol>> Functions { get; init; }

    public required List<ISymbol> Names { get; init; }

    public required List<ISymbol> Statics { get; init; }

    public required List<List<ISymbol>> Structs { get; init; }

    public required List<ISymbol> Typedefs { get; init; }

    public required List<List<ISymbol>> Unions { get; init; }

    public void Parse()
    {
        ParseTypedefs();
    }

    private void ParseTypedefs()
    {
        if (false)
        {
            foreach (var def in Typedefs.Where(s => s is ISymbolDefinition and not ISymbolDefinition2).Cast<ISymbolDefinition>()) // TODO sucks
            {
                var str = TypedefUtility.Parse(def);

                Console.WriteLine($"{def,-70} -> {str}");
            }
        }

        foreach (var def in Typedefs.OfType<ISymbolDefinition2>())
        {
            var str = TypedefUtility.Parse(def);

            continue;
            Console.WriteLine($"{def,-80} -> {str}");
        }
    }
}