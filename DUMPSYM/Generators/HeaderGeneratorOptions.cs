namespace DUMPSYM.Generators;

public sealed class HeaderGeneratorOptions
{
    public HashSet<string> RemoveTypedefs { get; init; } = [];
}