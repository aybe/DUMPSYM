namespace DUMPSYM.Generators;

public sealed class IdaHeaderGeneratorOptions
{
    public HashSet<string> RemoveTypedefs { get; init; } = [];
}