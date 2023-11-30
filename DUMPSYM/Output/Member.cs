namespace DUMPSYM.Output;

public sealed class Member(string name)
{
    public string Name { get; } = name;

    public string Text { get; init; } = null!; // TODO this sucks

    public override string ToString()
    {
        return $"{nameof(Name)}: {Name}";
    }
}