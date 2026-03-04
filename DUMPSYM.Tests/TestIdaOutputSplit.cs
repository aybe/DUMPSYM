using DUMPSYM.Tests.IDA;

// ReSharper disable IdentifierTypo

namespace DUMPSYM.Tests;

[TestClass]
public sealed class TestIdaOutputSplit : TestBase
{
    [TestMethod]
    [DynamicData(nameof(GetDynamicTestData), DynamicDataDisplayName = nameof(GetDynamicTestName), DynamicDataDisplayNameDeclaringType = typeof(TestBase))]
    public void Test(string sourcePath, string targetPath)
    {
        var text = File.ReadAllText(sourcePath);

        var output = IdaOutput.Parse(text);

        foreach (var function in output.Functions)
        {
            WriteLineVar(function.Name);
            WriteLineVar(function.Code.Lines);
            WriteLineVar(function.Comments.Lines);
            WriteLineVar(function.Code.Value);
            WriteLineVar(function.Comments.Value);
            WriteLine();
        }
    }
}