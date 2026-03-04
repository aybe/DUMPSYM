using DUMPSYM.Symbols;

namespace DUMPSYM.Tests;

[TestClass]
public sealed class TestCompareOutputs : TestBase
{
    [TestMethod]
    [DynamicData(nameof(GetDynamicTestData), DynamicDataDisplayName = nameof(GetDynamicTestName), DynamicDataDisplayNameDeclaringType = typeof(TestBase))]
    public void TestCompare(string sourcePath, string targetPath)
    {
        var source = GetSourceText(sourcePath);

        var target = GetTargetText(sourcePath);

        TextComparer.CompareLineByLine(source, target, Console.WriteLine);
    }

    private static string GetSourceText(string path)
    {
        using var writer = new StringWriter();

        var previous = Console.Out;

        Console.SetOut(writer);

        Original.__main(["", path]);

        Console.SetOut(previous);

        var text = writer.ToString();

        return text;
    }

    private static string GetTargetText(string path)
    {
        using var stream = File.OpenRead(path);

        var file = SymbolFile.Dump(stream);

        var text = file.ToString();

        return text;
    }
}