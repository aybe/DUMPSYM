namespace DUMPSYM.Tests;

[TestClass]
public sealed class UnitTest1
{
    [TestMethod]
    public void TestMethod1()
    {
        const string sym = """C:\Files\GitHub\psx_mnd_sym\cmd\sym_dump\main.sym""";
        const string txt = """C:\Files\GitHub\psx_mnd_sym\cmd\sym_dump\main.txt""";

        using var stream = File.OpenRead(sym);

        var file = SymbolFile.Dump(stream);

        var expected = File.ReadAllText(txt);
        var actual = file.ToString();

        var line = 0;
        using var sourceReader = new StringReader(expected);
        using var targetReader = new StringReader(actual);

        while (true)
        {
            var sourceLine = sourceReader.ReadLine();
            var targetLine = targetReader.ReadLine();

            if (sourceLine is null && targetLine is null)
            {
                break;
            }

            if (sourceLine != targetLine)
            {
                Assert.Fail($"\nLine {line + 1}\nSource: {sourceLine}\nTarget: {targetLine}");
            }

            line++;
        }
    }
}