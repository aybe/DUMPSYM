namespace DUMPSYM.Tests.IDA;

public sealed record IdaOutputFunction(string Name, IdaOutputChunk Code, IdaOutputChunk Comments)
{
    public override string ToString()
    {
        return Name;
    }
}