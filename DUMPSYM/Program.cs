// TODO cleanup/DRY API
// BUG: figure out why hot reload fails at start although nothing was changed: new stuff = embedded resource

using System.CommandLine;
using DUMPSYM.Generators;
using DUMPSYM.Symbols;

namespace DUMPSYM;

internal static class Program
{
    public static int Main(string[] args)
    {
        var root = new RootCommand("dumpsym 2.02 (c) 1997 SN Systems Software Ltd")
        {
            GetDumpCommand(),
            GetHeaderCommand(),
            GetScriptsCommand(),
            GetSplitCommand(),
        };

        return root.Parse(args).Invoke();
    }

    #region dump

    private static Command GetDumpCommand()
    {
        var cmd = new Command("dump") { Description = "Dump .SYM file to stdout" };

        var symArg = new Argument<FileInfo>("source-sym") { Description = "Source .SYM file" };

        symArg.AcceptExistingOnly().AcceptLegalFileNamesOnly();

        cmd.Add(symArg);

        cmd.SetAction(result =>
        {
            var sym = result.GetRequiredValue(symArg);

            RunDumpCommand(sym);
        });

        return cmd;
    }

    private static void RunDumpCommand(FileInfo sym)
    {
        var file = SymbolFile.Dump(sym.FullName);

        Console.WriteLine(file);
    }

    #endregion

    #region header

    private static Command GetHeaderCommand()
    {
        // TODO symbol cleaner options

        var cmd = new Command("header") { Description = "Generate .H file from .SYM file" };

        var symArg = new Argument<FileInfo>("source-sym") { Description = "Source .SYM file" };

        var hdrArg = new Argument<FileInfo>("target-h") { Description = "Target .H file" };

        symArg.AcceptExistingOnly().AcceptLegalFileNamesOnly();

        hdrArg.AcceptLegalFileNamesOnly();

        cmd.Add(symArg);

        cmd.Add(hdrArg);

        cmd.SetAction(result =>
        {
            var sym = result.GetRequiredValue(symArg);

            var hdr = result.GetRequiredValue(hdrArg);

            RunHeaderCommand(sym, hdr);
        });

        return cmd;
    }

    private static void RunHeaderCommand(FileInfo sym, FileInfo hdr)
    {
        var file = SymbolFile.Dump(sym.FullName);

        var header = IdaGeneratorUtility.GenerateHeader(file);

        File.WriteAllText(hdr.FullName, header);
    }

    #endregion

    #region scripts

    private static Command GetScriptsCommand()
    {
        var cmd = new Command("scripts") { Description = "Generate IDAPython scripts from .SYM file" };

        var symArg = new Argument<FileInfo>("source-sym") { Description = "Source .SYM file" };

        var dirArg = new Argument<DirectoryInfo>("target-dir") { Description = "Target directory" };

        symArg.AcceptExistingOnly().AcceptLegalFileNamesOnly();

        dirArg.AcceptExistingOnly().AcceptLegalFilePathsOnly();

        cmd.Add(symArg);

        cmd.Add(dirArg);

        cmd.SetAction(result =>
        {
            var sym = result.GetRequiredValue(symArg);

            var dir = result.GetRequiredValue(dirArg);

            RunScriptsCommand(sym, dir);
        });

        return cmd;
    }

    private static void RunScriptsCommand(FileInfo sym, DirectoryInfo dir)
    {
        IdaGeneratorUtility.GenerateScripts(SymbolFile.Dump(sym.FullName), dir.FullName);
    }

    #endregion

    #region split

    private static Command GetSplitCommand()
    {
        var cmd = new Command("split") { Description = "Split IDA .C file using files from .SYM file" };

        var srcArg = new Argument<FileInfo>("source-c") { Description = "Source .C file" };

        var symArg = new Argument<FileInfo>("source-sym") { Description = "Source .SYM file" };

        var dirArg = new Argument<DirectoryInfo>("target-dir") { Description = "Target directory" };

        srcArg.AcceptExistingOnly().AcceptLegalFileNamesOnly();

        symArg.AcceptExistingOnly().AcceptLegalFileNamesOnly();

        dirArg.AcceptExistingOnly().AcceptLegalFilePathsOnly();

        cmd.Add(srcArg);
        cmd.Add(symArg);
        cmd.Add(dirArg);

        cmd.SetAction(s =>
        {
            var src = s.GetRequiredValue(srcArg);
            var sym = s.GetRequiredValue(symArg);
            var dir = s.GetRequiredValue(dirArg);

            RunSplitCommand(src, sym, dir);
        });

        return cmd;
    }

    private static void RunSplitCommand(FileInfo src, FileInfo sym, DirectoryInfo dir)
    {
        Console.WriteLine(src.FullName);
        Console.WriteLine(sym.FullName);
        Console.WriteLine(dir.FullName);

        // TODO
    }

    #endregion
}