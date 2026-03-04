using DUMPSYM.Symbols;

namespace DUMPSYM;

internal static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length != 1)
        {    
            Console.WriteLine("""
                              dumpsym 2.02 (c) 1997 SN Systems Software Ltd
                              Usage: dumpsym sym_file
                              """);

            return 1;
        }

        var path = args[0];

        if (!File.Exists(path))
        {
            Console.WriteLine($"Error: Can't open file '{path}' for input");

            return 1;
        }

        try
        {
            using var stream = File.OpenRead(path);

            var file = SymbolFile.Dump(stream);

            var text = file.ToString();

            Console.WriteLine(text);

            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to parse .SYM file:");
            Console.WriteLine(e);

            return 1;
        }
    }
}