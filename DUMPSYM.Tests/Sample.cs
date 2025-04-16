namespace DUMPSYM.Tests;

public static class Sample
{
    static Sample()
    {
        var path = Path.Combine(Solution.Directory, "MAIN.SYM");

        using var stream = File.OpenRead(path);

        var file = SymbolFile.Dump(stream);

        Default = file;
    }

    public static SymbolFile Default { get; }
}