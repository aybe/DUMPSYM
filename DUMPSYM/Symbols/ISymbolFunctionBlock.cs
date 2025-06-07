namespace DUMPSYM.Symbols;

public interface ISymbolFunctionBlock : ISymbol
{
    uint Line { get; }
}