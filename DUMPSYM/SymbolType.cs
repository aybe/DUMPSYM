using Newtonsoft.Json;

namespace DUMPSYM;

[Serializable]
public record struct SymbolType
{
    public SymbolType(SymbolTypeKind kind, params SymbolTypeModifier[] modifiers)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(modifiers.Length, 6, nameof(modifiers));

        var output = (int)kind & 0xF;

        for (var i = 0; i < modifiers.Length; i++)
        {
            output |= ((int)modifiers[i] & 0b11) << (4 + 2 * i);
        }

        Value = (ushort)output;
    }

    public ushort Value { get; set; }

    [JsonIgnore]
    public SymbolTypeKind Kind => (SymbolTypeKind)(Value & 0xF);

    [JsonIgnore]
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