namespace DUMPSYM.Symbols;

public interface ISymbolDefinition2 : ISymbolDefinition
{
    uint[] Dimensions { get; }

    string Tag { get; set; }
}