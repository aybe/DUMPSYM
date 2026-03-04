namespace DUMPSYM.Tests;

[Serializable]
public sealed class TestPair
{
    public string Source { get; set; } = null!;

    public string Target { get; set; } = null!;

    public void Deconstruct(out string source, out string target)
    {
        source = Source;
        target = Target;
    }
}