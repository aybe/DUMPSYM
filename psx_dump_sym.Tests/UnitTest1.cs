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
        
        SymbolUtility.Dump(stream);
    }
}