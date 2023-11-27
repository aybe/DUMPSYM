namespace DUMPSYM;

public sealed class SymbolType
{
    public SymbolType(ushort value)
    {
        Kind = (SymbolTypeKind)(value & 0xF);

        for (var i = 0; i < 6; i++)
        {
            var modifier = (value >> (4 + 2 * i)) & 0b11;

            if (modifier is 0)
            {
                break;
            }

            Modifiers.Add((SymbolTypeModifier)modifier);
        }
    }

    public SymbolTypeKind Kind { get; }

    public IList<SymbolTypeModifier> Modifiers { get; } = new List<SymbolTypeModifier>();

    public override string ToString()
    {
        return $"{(Modifiers.Count > 0 ? $"{string.Join(" ", Modifiers)} " : string.Empty)}{Kind}";
    }
}