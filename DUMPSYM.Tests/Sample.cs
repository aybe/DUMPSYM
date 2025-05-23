namespace DUMPSYM.Tests;

public static class Sample
{
    static Sample()
    {
        const string path = @"C:\Files\GitHub\DUMPSYM\MAIN.SYM";

        using var stream = File.OpenRead(path);

        var file = SymbolFile.Dump(stream);

        Default = file;
    }

    public static SymbolFile Default { get; }
}