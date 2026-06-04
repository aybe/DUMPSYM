namespace DUMPSYM.Symbols;

public readonly record struct SymbolHeader(long Position, uint Address, byte Type)
{
    public override string ToString()
    {
        return $"{Position:x6}: ${Address:x8} {Type:x}";
    }
}