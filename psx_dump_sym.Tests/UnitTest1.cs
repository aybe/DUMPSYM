namespace psx_dump_sym.Tests;

[TestClass]
public class UnitTest1
{
    public static IEnumerable<object[]> DumpSymFileData()
    {
        var path = UnitTestDataUtility.GetFullPath(".SYM file list.txt");

        var text = File.ReadAllLines(path);

        foreach (var line in text)
        {
            if (File.Exists(line))
            {
                yield return new object[] { line };
            }
        }
    }

    [TestMethod]
    [DynamicData(nameof(DumpSymFileData), DynamicDataSourceType.Method)]
    public void DumpSymFile(string path)
    {
        using var stream = File.OpenRead(path);

        var file = SymbolUtility.Dump(stream);

        var format = file.ToString();
        var expected = File.ReadAllLines(Path.ChangeExtension(path, ".txt"))[..^1];
        var actual = format.ReadLines();

        for (var i = 0; i < Math.Min(expected.Length, actual.Length); i++)
        {
            var s = expected[i];
            var t = actual[i];
            var u = new string(s.GetCommonPrefix(t).ToArray());
            if (u != t)
            {
                Assert.AreEqual(s, t, "\nCurrent: " + u);
            }
        }

        CollectionAssert.AreEquivalent(expected, actual);

        Console.WriteLine(format);
    }
}