namespace DUMPSYM;

public interface ISymbolFunctionBlock : ISymbol
{
    uint Line { get; }
}