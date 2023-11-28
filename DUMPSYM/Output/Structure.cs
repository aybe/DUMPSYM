using System.Text;

namespace DUMPSYM.Output;

public sealed class Structure(string name)
{
    public string Name { get; } = name;

    public IList<Member> Members { get; } = new List<Member>();

    public string Print(StringBuilder? builder = null)
    {
        builder ??= new StringBuilder();

        builder.AppendLine($"struct {Name}");

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