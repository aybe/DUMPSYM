using System.Collections;
using JetBrains.Annotations;

namespace DUMPSYM.Tests;

[NoReorder]
public sealed class SymKey : IReadOnlyList<string>, IEquatable<SymKey>
{
    public SymKey(params object[] names)
    {
        foreach (var name in names)
        {
            if (name is null)
            {
                throw new ArgumentOutOfRangeException(nameof(names));
            }

            Names.Add(name.ToString()!);
        }
    }

    private List<string> Names { get; } = [];

    public string this[int index] => Names[index];

    public int Count => Names.Count;

    public long Position { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            //return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((SymKey)obj);
    }

    public bool Equals(SymKey? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            //return true;
        }

        return Names.SequenceEqual(other.Names, StringComparer.Ordinal);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)Names).GetEnumerator();
    }

    public IEnumerator<string> GetEnumerator()
    {
        return Names.GetEnumerator();
    }

    public override int GetHashCode()
    {
        var code = new HashCode();

        foreach (var name in Names)
        {
            code.Add(name);
        }

        return code.ToHashCode();
    }

    public override string ToString()
    {
        return $"[{string.Join(", ", Names)}]";
    }

    public static bool operator ==(SymKey? left, SymKey? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(SymKey? left, SymKey? right)
    {
        return !Equals(left, right);
    }
}