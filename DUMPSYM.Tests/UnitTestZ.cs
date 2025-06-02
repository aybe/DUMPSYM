using DUMPSYM.Generators;

namespace DUMPSYM.Tests;

[TestClass]
public sealed class UnitTestZ : UnitTestBase
{
    [TestMethod]
    public void TestMethodY()
    {
        using var generator = new IdaFunctionGenerator();

        generator.Initialize(Sample.Default);
    }
}