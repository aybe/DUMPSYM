using DUMPSYM.Symbols;

namespace DUMPSYM.Tests;

[TestClass]
public sealed class TestDumper : TestBase
{
    [TestMethod]
    [DynamicData(nameof(GetDynamicTestData), DynamicDataDisplayName = nameof(GetDynamicTestName), DynamicDataDisplayNameDeclaringType = typeof(TestBase))]
    public void TestOriginalVsManagedOutput(string sourcePath, string targetPath)
    {
        var sourceText = GetSourceText(sourcePath);

        var targetText = GetTargetText(sourcePath);

        TextComparer.CompareLineByLine(sourceText, targetText, Console.WriteLine);

        return;

        static string GetSourceText(string path)
        {
            using var writer = new StringWriter();

            var previous = Console.Out;

            Console.SetOut(writer);

            Original.__main(["", path]);

            Console.SetOut(previous);

            var text = writer.ToString();

            return text;
        }

        static string GetTargetText(string path)
        {
            using var stream = File.OpenRead(path);

            var file = SymbolFile.Dump(stream);

            var text = file.ToString();

            return text;
        }
    }
}