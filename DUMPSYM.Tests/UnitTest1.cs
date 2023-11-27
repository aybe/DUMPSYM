using System.Reflection;

namespace DUMPSYM.Tests;

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

    public static string DumpSymFileName(MethodInfo methodInfo, object[] data)
    {
        return $"{methodInfo.Name} {Path.GetFileName(data[0].ToString()!)}";
    }

    [TestMethod]
    [DynamicData(nameof(DumpSymFileData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(DumpSymFileName))]
    public void DumpSymFile(string path)
    {
        using var stream = File.OpenRead(path);

        var file = SymbolFile.Dump(stream);

        var text = file.ToString();

        Console.WriteLine(text);

        var textFile = Path.ChangeExtension(path, ".txt");

        if (!Path.Exists(textFile))
        {
            Assert.Inconclusive("Can't compare against original, no associated .txt file was found.");
            return;
        }

        var expected = File.ReadAllLines(textFile)[..^1];
        var actual = text.ReadLines();

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
    }
}