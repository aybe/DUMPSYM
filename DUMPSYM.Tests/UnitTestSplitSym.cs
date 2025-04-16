using System.Text.RegularExpressions;

namespace DUMPSYM.Tests;

[TestClass]
public sealed class UnitTestSplitSym : UnitTestBase
{
    private static Regex RegexMatchFile { get; } = new(
        @"^[0-9a-f]{6}:\s\$[0-9a-f]{8}\s\d{2}\sSet\sSLD\sto\sline\s\d+\sof\sfile\s(?<path>(?:\w:\\)(?:(?:\w+\\)+)(?:\w+(?:\.\w{1,3})?))");

    private static Regex RegexSplitFile { get; } = new(
        @"^(?=[0-9a-f]{6}:\s\$[0-9a-f]{8}\s\d{2}\sSet\sSLD\sto\sline\s\d+\sof\sfile\s\w:\\(?:\w+\\)+(?:\w+(?:\.\w{1,3})?)\r?$)",
        RegexOptions.Multiline);

    [TestMethod]
    public void SplitSymFileBySldFile()
    {
        var sourceFile = Path.GetFullPath(Path.Combine(Solution.Directory, "MAIN.SYM.txt"));

        var targetDirectory = Constants.TestDataDirectory;

        var input = File.ReadAllText(sourceFile);

        var split = RegexSplitFile.Split(input);

        foreach (var block in split)
        {
            var match = RegexMatchFile.Match(block);

            if (match.Success)
            {
                var name = Path.GetFileName(match.Groups["path"].Value);

                var path = Path.Combine(targetDirectory, $"{name}.txt");

                File.WriteAllText(path, block);
            }
        }
    }
}