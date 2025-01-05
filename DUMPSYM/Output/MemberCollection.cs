using System.Text;

namespace DUMPSYM.Output;

public sealed class MemberCollection
{
    public MemberCollection(string keyword, string name)
    {
        Keyword = keyword;
        Name = name;
    }

    public string Keyword { get; }

    public string Name { get; }

    public IList<Member> Members { get; } = new List<Member>();

    public string Print(StringBuilder? builder = null)
    {
        builder ??= new StringBuilder();

        builder.AppendLine($"{Keyword} {Name}");

        builder.AppendLine("{");

        foreach (var member in Members)
        {
            builder.AppendLine($"\t{member.Text}");
        }

        builder.AppendLine("};");

        return builder.ToString();
    }

    public override string ToString()
    {
        return $"{nameof(Name)}: {Name}, {nameof(Members)}: {Members.Count}";
    }
}