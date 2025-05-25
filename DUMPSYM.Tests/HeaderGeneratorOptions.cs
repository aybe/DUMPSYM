namespace DUMPSYM.Tests;

public sealed class HeaderGeneratorOptions
{
    public HashSet<string> RemoveTypedefs { get; init; } = [];

    public bool UseSdkUnsignedTypedefs { get; init; }
}