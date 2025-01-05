namespace DUMPSYM.Output;

public sealed class Member
{
    public Member(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public string Text { get; init; } = null!; // TODO this sucks

    public override string ToString()
    {
        return $"{nameof(Name)}: {Name}";
    }
}