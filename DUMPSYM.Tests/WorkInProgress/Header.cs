using System.Collections;
using System.Collections.Immutable;

namespace DUMPSYM.Tests.WorkInProgress;

public sealed record Header : IEnumerable<Header>
{
    public Header(string path, params Header[] dependencies)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentOutOfRangeException(nameof(path), "Path cannot be null or whitespace.");
        }

        if (dependencies.Length != 0)
        {
            var headers = new HashSet<Header>(dependencies);

            if (dependencies.Length != headers.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(dependencies), "Dependencies must be distinct.");
            }
        }

        Path = path;

        Dependencies = [..dependencies];
    }

    public string Path { get; }

    public ImmutableArray<Header> Dependencies { get; }

    public IEnumerator<Header> GetEnumerator()
    {
        return GetDependencies().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool Equals(Header? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Path == other.Path && Dependencies.SequenceEqual(other.Dependencies);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Path, Dependencies);
    }

    public override string ToString()
    {
        return $"{Path}, {nameof(Dependencies)}: {string.Join(", ", GetDependencies().Select(s => s.Path))}";
    }

    private HashSet<Header> GetDependencies()
    {
        var set = new HashSet<Header>();

        var stack = new Stack<Header>(Dependencies.Reverse());

        while (stack.Count > 0)
        {
            var pop = stack.Pop();

            if (!set.Add(pop))
            {
                continue;
            }

            foreach (var item in pop.Dependencies.Reverse())
            {
                stack.Push(item);
            }
        }

        return set;
    }
}