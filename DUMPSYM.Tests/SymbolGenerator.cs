namespace DUMPSYM.Tests;

public sealed class SymbolGenerator(SymbolFactory factory)
{
    private SymbolFactory Factory { get; } = factory;

    private bool GenerateTypeEnabled { get; } = true;

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

    private static string GenerateTypeDefinition(Symbol symbol)
    {
        var value = $"TYPEDEF = Name: {symbol.Name}, Tag: {symbol.Tag}";

        return value;
    }

    private void GenerateTypeDefinitions(SortedDictionary<long, string> dictionary)
    {
        foreach (var symbol in Factory.DistinctTypeDefinition)
        {
            Assert.AreEqual(SymbolStorageClass.TPDEF, symbol.Class);

            var value = GenerateTypeDefinition(symbol);

            dictionary.Add(symbol.Header.Position, value);
        }
    }
}