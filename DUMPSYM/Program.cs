using System.CommandLine;
using DUMPSYM.Symbols;

namespace DUMPSYM;

internal static class Program
{
    public static int Main(string[] args)
    {
        var path = new Argument<FileInfo>("sym_file");

        var ida = new Option<bool>("--ida") { Description = "Generate IDA scripts" };

        var root = new RootCommand("dumpsym 2.02 (c) 1997 SN Systems Software Ltd") { path, ida };

        var result = root.Parse(args);

        if (result.Action != null)
        {
            return result.Invoke();
        }

        var info = result.GetRequiredValue(path);

        if (!info.Exists)
        {
            Console.WriteLine($"Error: Can't open file '{info.FullName}' for input");

            return 1;
        }

        using var stream = info.OpenRead();

        var file = SymbolFile.Dump(stream);

        if (result.GetValue(ida))
        {
            // TODO
        }
        else
        {
            var text = file.ToString();

            Console.WriteLine(text);
        }

        return 0;
    }
}