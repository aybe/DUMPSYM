namespace DUMPSYM.Tests;

[TestClass]
public sealed class UnitTest1
{
    public static SymbolFile GetSample() // TODO more
    {
        const string pa = """C:\Files\GitHub\psx_mnd_sym\cmd\sym_dump\main.sym""";

        using var stream = File.OpenRead(pa);

        var file = SymbolFile.Dump(stream);

        return file;
    }
}