using System.Collections;

namespace DUMPSYM;

public sealed class Code : IReadOnlyList<ISymbol>
{
    public Code(ISymbol symbol)
    {
        Symbols = [symbol];
    }

    public Code(List<ISymbol> symbols)
    {
        Symbols = symbols;
    }

    public List<ISymbol> Symbols { get; }

    public IEnumerator<ISymbol> GetEnumerator()
    {
        return Symbols.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)Symbols).GetEnumerator();
    }

    public int Count => Symbols.Count;

    public ISymbol this[int index] => Symbols[index];

    public List<ISymbol> Slice(int start, int length) // this[Range]
    {
        return Symbols.GetRange(start, length);
    }

    public override string ToString()
    {
        return this[0].ToString()!;
    }
}