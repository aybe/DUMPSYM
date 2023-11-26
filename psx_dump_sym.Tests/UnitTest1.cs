namespace psx_dump_sym.Tests;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void TestMethod1()
    {
        const string path = @"C:\GitHub\Hi-Octane\MAIN.SYM";

        using var stream = File.OpenRead(path);

        SymbolUtility.Dump(stream);
    }
}