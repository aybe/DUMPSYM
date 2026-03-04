using DUMPSYM.Tests.IDA;

// ReSharper disable IdentifierTypo

namespace DUMPSYM.Tests;

[TestClass]
public sealed class TestIdaOutputSplit : TestBase
{
    [TestMethod]
    public void Test()
    {
        var text = File.ReadAllText(@"C:\GitHub\HigherOctane\TEMP\001-only-types-and-functions-applied.c");

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