namespace DUMPSYM;

public sealed record SymbolPriority(int Value = default, string? Label = "UNNAMED")
{
    public bool Equals(SymbolPriority? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value;
    }

    public override string ToString()
    {
        return $"{Value} ({Label})";
    }

    public static implicit operator int(SymbolPriority priority)
    {
        return priority.Value;
    }

    public static SymbolPriority operator +(SymbolPriority priority, int value)
    {
        return priority with { Value = priority.Value + value };
    }

    public static SymbolPriority operator -(SymbolPriority priority, int value)
    {
        return priority with { Value = priority.Value - value };
    }

    public static SymbolPriority operator ++(SymbolPriority priority)
    {
        return priority with { Value = priority.Value + 1 };
    }

    public static SymbolPriority operator --(SymbolPriority priority)
    {
        return priority with { Value = priority.Value - 1 };
    }
}