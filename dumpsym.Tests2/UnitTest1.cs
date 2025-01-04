namespace dumpsym.Tests;

[TestClass]
public class UnitTest1
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    public void TestMethod1()
    {
        const string sourcePath = @"C:\Temp\MAIN.SYM";
        const string targetPath = @"C:\Temp\MAIN.SYM.TXT";

        CompareDumps(sourcePath, targetPath);
    }

    public static void CompareDumps(string sourcePath, string targetPath)
    {
        string sourceOutput;

        using (var consoleCapture = new ConsoleCapture())
        {
            consoleCapture.OutputToSource = false;
            consoleCapture.OutputToTarget = true;

            Globals.__main(new[] { "", sourcePath });

            sourceOutput = consoleCapture.ToString();
        }

        var targetOutput = File.ReadAllText(targetPath);

        CompareLines(sourceOutput, targetOutput);
    }

    public static void CompareLines(string source, string target)
    {
        var separator = new[] { "\r\n", "\r", "\n" };

        const StringSplitOptions options = StringSplitOptions.None;

        var sourceLines = source.Split(separator, options);
        var targetLines = target.Split(separator, options);

        var sourceCount = sourceLines.Length;
        var targetCount = targetLines.Length;

        Console.WriteLine($"Source lines: {sourceCount}");
        Console.WriteLine($"Target lines: {targetCount}");

        Assert.AreEqual(sourceCount, targetCount, "Lines count do not match.");

        var line = 0;

        var list = new List<(int LineIndex, string SourceLine, string TargetLine)>();

        for (var i = 0; i < sourceCount; i++)
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
            Console.WriteLine(
                $"Wrong match @ {lineIndex}:\n" +
                $"\tSource: \"{sourceLine}\"\n" +
                $"\tTarget: \"{targetLine}\"");
        }

        var nPass = sourceCount - list.Count;
        var nFail = list.Count;
        var count = 1.0d / sourceCount;

        Assert.AreEqual(0, nFail, $"PASS = {nPass} ({count * nPass:P}), FAIL = {nFail} ({count * nFail:P})");
    }
}