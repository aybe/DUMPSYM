namespace DUMPSYM;

public interface ISymbolFileStart : ISymbol
{
    string File { get; set; }
}