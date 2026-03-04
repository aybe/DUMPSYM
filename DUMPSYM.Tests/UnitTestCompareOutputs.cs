using DUMPSYM.Symbols;

namespace DUMPSYM.Tests;

[TestClass]
public sealed class UnitTestCompareOutputs : UnitTestBase
{
    [TestMethod]
    [DynamicData(nameof(GetTestData), DynamicDataDisplayName = nameof(GetTestName), DynamicDataDisplayNameDeclaringType = typeof(UnitTestBase))]
    public void TestCompareOutputs(string sourcePath, string targetPath)
    {
        var source = GetSourceText(sourcePath);

        var target = GetTargetText(sourcePath);

        CompareLines(source, target);
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

    private void CompareLines(string sourceText, string targetText)
    {
        var separator = new[] { "\r\n", "\r", "\n" };

        const StringSplitOptions options = StringSplitOptions.None;

        var sourceLines = sourceText.Split(separator, options);
        var targetLines = targetText.Split(separator, options);

        Assert.HasCount(sourceLines.Length, targetLines, "Lines count don't match.");

        var line = 0;

        var list = new List<(int LineIndex, string SourceLine, string TargetLine)>();

        for (var i = 0; i < sourceLines.Length; i++)
        {
            var sourceLine = sourceLines[i];
            var targetLine = targetLines[i];

            if (!string.Equals(sourceLine, targetLine, StringComparison.Ordinal))
            {
                list.Add((line, sourceLine, targetLine));
            }

            line++;
        }

        foreach (var (lineIndex, sourceLine, targetLine) in list)
        {
            WriteLine(
                $"Line {lineIndex} doesn't match:\n" +
                $"\tSource: \"{sourceLine}\"\n" +
                $"\tTarget: \"{targetLine}\"");
        }

        var pass = sourceLines.Length - list.Count;
        var fail = list.Count;

        Assert.AreEqual(0, fail,
            $"PASS = {pass} ({(double)pass / sourceLines.Length:P}), " +
            $"FAIL = {fail} ({(double)fail / sourceLines.Length:P})");
    }
}