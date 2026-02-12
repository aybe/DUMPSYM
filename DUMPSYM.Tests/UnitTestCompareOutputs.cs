using System.Text;
using DUMPSYM.Symbols;

namespace DUMPSYM.Tests;

[TestClass]
public sealed class UnitTestCompareOutputs : UnitTestBase
{
    [TestMethod]
    [DataRow(@"C:\GitHub\DUMPSYM\MAIN.SYM")]
    public void TestCompareOutputs(string path)
    {
        string source;

        using (var consoleCapture = new ConsoleCapture())
        {
            consoleCapture.OutputToSource = false;
            consoleCapture.OutputToTarget = true;

            Original.__main(["", path]);

            source = consoleCapture.ToString();
        }

        using var stream = File.OpenRead(path);

        var file = SymbolFile.Dump(stream);

        var target = file.ToString();

        CompareLines(source, target);
    }

    private void CompareLines(string source, string target)
    {
        var separator = new[] { "\r\n", "\r", "\n" };

        const StringSplitOptions options = StringSplitOptions.None;

        var sourceLines = source.Split(separator, options);
        var targetLines = target.Split(separator, options);

        var sourceCount = sourceLines.Length;
        var targetCount = targetLines.Length;

        WriteLine($"Source lines: {sourceCount}");
        WriteLine($"Target lines: {targetCount}");

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
            WriteLine(
                $"Wrong match @ {lineIndex}:\n" +
                $"\tSource: \"{sourceLine}\"\n" +
                $"\tTarget: \"{targetLine}\"");
        }

        var nPass = sourceCount - list.Count;
        var nFail = list.Count;
        var count = 1.0d / sourceCount;

        Assert.AreEqual(0, nFail, $"PASS = {nPass} ({count * nPass:P}), FAIL = {nFail} ({count * nFail:P})");
    }

    private sealed class ConsoleCapture : TextWriter
    {
        public ConsoleCapture()
        {
            Source = Console.Out;

            Target = new StringWriter();

            Console.SetOut(this);
        }

        private TextWriter Source { get; }

        private StringWriter Target { get; }

        public override Encoding Encoding => Encoding.UTF8;

        public bool OutputToSource { get; set; } = true;

        public bool OutputToTarget { get; set; } = true;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            Console.SetOut(Source);
        }

        public override void Flush()
        {
            base.Flush();

            Source.Flush();

            Target.Flush();
        }

        public override void Write(char value)
        {
            if (OutputToSource)
            {
                Source.Write(value);
            }

            if (OutputToTarget)
            {
                Target.Write(value);
            }
        }

        public override string ToString()
        {
            Flush();

            var s = Target.ToString();

            return s;
        }
    }
}