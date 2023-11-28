using System.Text;

namespace DUMPSYM.Output;

public sealed class MemberCollection(string keyword, string name)
{
    public string Keyword { get; } = keyword;

    public string Name { get; } = name;

    public IList<Member> Members { get; } = new List<Member>();

    public string Print(StringBuilder? builder = null)
    {
        builder ??= new StringBuilder();

        builder.AppendLine($"{keyword} {Name}");

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