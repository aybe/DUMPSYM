namespace DUMPSYM.Tests;

[TestClass]
public sealed class UnitTestY : UnitTestBase
{
    [TestMethod]
    public void TestMethodY()
    {
        var directory = Path.Combine(Solution.Directory, "Project1", "src");

        Directory.CreateDirectory(directory);

        using var generator = new Generator(directory);

        generator.Generate(Sample.Default.Symbols.ToArray());

        generator.Write();

        WriteLine(generator.GetStatistics());


    }
}