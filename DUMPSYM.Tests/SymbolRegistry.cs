namespace DUMPSYM.Tests;

public sealed class SymbolRegistry
{
    public IList<Symbol> Externals { get; } = new List<Symbol>();

    public IList<Symbol> Files { get; } = new List<Symbol>();

    public IList<Symbol> Registers { get; } = new List<Symbol>();

    public IList<Symbol> Statics { get; } = new List<Symbol>();

    public IList<Symbol> TypeDefinitions { get; } = new List<Symbol>();

    public IList<LinkedList<Symbol>> Enumerations { get; } = new List<LinkedList<Symbol>>();

    public IList<LinkedList<Symbol>> Structures { get; } = new List<LinkedList<Symbol>>();

    public IList<LinkedList<Symbol>> Unions { get; } = new List<LinkedList<Symbol>>();
}