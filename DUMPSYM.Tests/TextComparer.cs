namespace DUMPSYM.Tests;

public static class TextComparer
{
    public static void CompareLineByLine(string source, string target, Action<string?> logger)
    {
        var separator = new[] { "\r\n", "\r", "\n" };

        const StringSplitOptions options = StringSplitOptions.None;

        var sourceLines = source.Split(separator, options);
        var targetLines = target.Split(separator, options);

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
            logger(
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