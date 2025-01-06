using JetBrains.Annotations;

namespace DUMPSYM;

public readonly record struct SymbolType
{
    [UsedImplicitly]
    private ushort Value { get; }

    public SymbolTypeKind Kind => (SymbolTypeKind)(Value & 0xF);

    public IEnumerable<SymbolTypeModifier> Modifiers
    {
        get
        {
            for (var i = 0; i < 6; i++)
            {
                var bits = (Value >> (4 + 2 * i)) & 0b11;

                if (bits is 0)
                {
                    yield break;
                }

                var modifier = (SymbolTypeModifier)bits;

                yield return modifier;
            }
        }
    }

    public override string ToString()
    {
        return string.Join(" ", string.Join(" ", Modifiers), Kind).Trim();
    }
}