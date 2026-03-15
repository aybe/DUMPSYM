using System.CommandLine;
using DUMPSYM.Symbols;

namespace DUMPSYM;

internal static class Program
{
    public static int Main(string[] args)
    {
        var root = new RootCommand("dumpsym 2.02 (c) 1997 SN Systems Software Ltd")
        {
            GetIdaCommand(),
            GetSymCommand(),
        };

        return root.Parse(args).Invoke();
    }

    #region IDA

    private static Command GetIdaCommand()
    {
        var ida = new Command("ida") { Description = "IDA related commands" };

        ida.Add(GetIdaSplitCommand());

        ida.Add(GetIdaScriptsCommand());

        return ida;
    }

    private static Command GetIdaScriptsCommand()
    {
        var root = new Command("scripts") { Description = "Generate IDA scripts from a .SYM file" };

        var symArg = new Argument<FileInfo>("source_sym") { Description = "Input .SYM file" };

        root.Add(symArg);

        root.SetAction(result =>
        {
            var sym = result.GetRequiredValue(symArg);

            RunIdaScriptsCommand(sym);
        });

        return root;
    }

    private static Command GetIdaSplitCommand()
    {
        var cmd = new Command("split") { Description = "Split output .C file" };

        var srcArg = new Argument<FileInfo>("source_c") { Description = "Source .C file" };

        var symArg = new Argument<FileInfo>("source_sym") { Description = "Source .SYM file" };

        var dirArg = new Argument<DirectoryInfo>("target_dir") { Description = "Target directory" };

        srcArg.Validators.Add(_ => srcArg.AcceptExistingOnly());
        srcArg.Validators.Add(_ => srcArg.AcceptLegalFileNamesOnly());

        symArg.Validators.Add(_ => symArg.AcceptExistingOnly());
        symArg.Validators.Add(_ => symArg.AcceptLegalFileNamesOnly());

        dirArg.Validators.Add(_ => dirArg.AcceptExistingOnly());
        dirArg.Validators.Add(_ => dirArg.AcceptLegalFilePathsOnly());

        cmd.Add(srcArg);
        cmd.Add(symArg);
        cmd.Add(dirArg);

        cmd.SetAction(s =>
        {
            var src = s.GetRequiredValue(srcArg);
            var sym = s.GetRequiredValue(symArg);
            var dir = s.GetRequiredValue(dirArg);

            RunIdaSplitCommand(src, sym, dir);
        });

        return cmd;
    }

    private static void RunIdaScriptsCommand(FileInfo sym)
    {
        Console.WriteLine(sym.FullName);

        // TODO
    }

    private static void RunIdaSplitCommand(FileInfo src, FileInfo sym, DirectoryInfo dir)
    {
        Console.WriteLine(src.FullName);
        Console.WriteLine(sym.FullName);
        Console.WriteLine(dir.FullName);

        // TODO
    }

    #endregion

    #region SYM

    private static Command GetSymCommand()
    {
        var cmd = new Command("sym") { Description = "SYM related commands" };

        cmd.Add(GetSymDumpCommand());

        return cmd;
    }

    private static Command GetSymDumpCommand()
    {
        var cmd = new Command("dump") { Description = ".SYM file dumper" };

        var symArg = new Argument<FileInfo>("sym") { Description = ".SYM file" };

        cmd.Add(symArg);

        cmd.SetAction(result =>
        {
            var sym = result.GetRequiredValue(symArg);

            RunSymDumpCommand(sym);
        });

        return cmd;
    }

    private static void RunSymDumpCommand(FileInfo sym)
    {
        using var stream = sym.OpenRead();

        var file = SymbolFile.Dump(stream);

        Console.WriteLine(file.ToString());
    }

    #endregion
}