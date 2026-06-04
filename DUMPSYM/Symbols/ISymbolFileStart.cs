namespace DUMPSYM.Symbols;

public interface ISymbolFileStart : ISymbol
{
    string File { get; }
}