using System.Diagnostics.CodeAnalysis;

namespace DUMPSYM.Tests;

[SuppressMessage("Style", "IDE0028:Simplify collection initialization", Justification = "buggy")]
public sealed class SymbolRegistry
{
    public List<Symbol> Externals { get; } = new();

    public List<Symbol> Files { get; } = new();

    public List<Symbol> Registers { get; } = new();

    public List<Symbol> Statics { get; } = new();

    public List<Symbol> TypeDefinitions { get; } = new();

    public List<LinkedList<Symbol>> Enumerations { get; } = new();

    public List<LinkedList<Symbol>> Functions { get; } = new();

    public List<LinkedList<Symbol>> Structures { get; } = new();

    public List<LinkedList<Symbol>> Unions { get; } = new();
}