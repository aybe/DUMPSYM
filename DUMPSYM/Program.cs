// TODO cleanup/DRY API
// BUG: figure out why hot reload fails at start although nothing was changed: new stuff = embedded resource

using System.CommandLine;
using System.Reflection;
using System.Text;
using DUMPSYM.Generators;
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

    #region ida

    private static Command GetIdaCommand()
    {
        var ida = new Command("ida") { Description = "IDA related commands" };

        ida.Add(GetIdaSplitCommand());

        ida.Add(GetIdaScriptsCommand());

        return ida;
    }

    #region ida scripts

    private static Command GetIdaScriptsCommand()
    {
        var cmd = new Command("scripts") { Description = "Generate IDA scripts from .SYM file" };

        var symArg = new Argument<FileInfo>("source_sym") { Description = "Source .SYM file" };

        var dirArg = new Argument<DirectoryInfo>("target_dir") { Description = "Target directory" };

        symArg.AcceptExistingOnly().AcceptLegalFileNamesOnly();

        dirArg.AcceptExistingOnly().AcceptLegalFilePathsOnly();

        cmd.Add(symArg);

        cmd.Add(dirArg);

        cmd.SetAction(result =>
        {
            var sym = result.GetRequiredValue(symArg);

            var dir = result.GetRequiredValue(dirArg);

            RunIdaScriptsCommand(sym, dir);
        });

        return cmd;
    }

    private static void RunIdaScriptsCommand(FileInfo sym, DirectoryInfo dir)
    {
        IdaGeneratorUtility.GenerateScripts(SymbolFile.Dump(sym.FullName), dir.FullName);

        // TODO utility should write main script instead

        var combine = Path.Combine(dir.FullName, "dumpsym.py");

        var contents = GetPythonScript();

        File.WriteAllText(combine, contents);

        return;

        static string GetPythonScript()
        {
            var assembly = Assembly.GetExecutingAssembly();

            using var stream = assembly.GetManifestResourceStream("DUMPSYM.dumpsym.py")!;

            using var reader = new StreamReader(stream, Encoding.UTF8);

            var s = reader.ReadToEnd();

            return s;
        }
    }

    #endregion

    #region ida split

    private static Command GetIdaSplitCommand()
    {
        var cmd = new Command("split") { Description = "Split output .C file" };

        var srcArg = new Argument<FileInfo>("source_c") { Description = "Source .C file" };

        var symArg = new Argument<FileInfo>("source_sym") { Description = "Source .SYM file" };

        var dirArg = new Argument<DirectoryInfo>("target_dir") { Description = "Target directory" };

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

            RunIdaSplitCommand(src, sym, dir);
        });

        return cmd;
    }

    private static void RunIdaSplitCommand(FileInfo src, FileInfo sym, DirectoryInfo dir)
    {
        Console.WriteLine(src.FullName);
        Console.WriteLine(sym.FullName);
        Console.WriteLine(dir.FullName);

        // TODO
    }

    #endregion

    #endregion

    #region sym

    private static Command GetSymCommand()
    {
        var cmd = new Command("sym") { Description = "SYM related commands" };

        cmd.Add(GetSymDumpCommand());

        cmd.Add(GetSymHeaderCommand());

        return cmd;
    }

    #region sym dump

    private static Command GetSymDumpCommand()
    {
        var cmd = new Command("dump") { Description = ".SYM file dumper" };

        var symArg = new Argument<FileInfo>("sym") { Description = ".SYM file" };

        symArg.AcceptExistingOnly().AcceptLegalFileNamesOnly();

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
        var file = SymbolFile.Dump(sym.FullName);

        Console.WriteLine(file);
    }

    #endregion

    #region sym header

    private static Command GetSymHeaderCommand()
    {
        // TODO symbol cleaner options

        var cmd = new Command("header") { Description = "Generate .H file from .SYM file" };

        var symArg = new Argument<FileInfo>("source.sym") { Description = "Source .SYM file" };

        var hdrArg = new Argument<FileInfo>("target.h") { Description = "Target .H file" };

        symArg.AcceptExistingOnly().AcceptLegalFileNamesOnly();

        hdrArg.AcceptLegalFileNamesOnly();

        cmd.Add(symArg);

        cmd.Add(hdrArg);

        cmd.SetAction(result =>
        {
            var sym = result.GetRequiredValue(symArg);

            var hdr = result.GetRequiredValue(hdrArg);

            RunSymHeaderCommand(sym, hdr);
        });

        return cmd;
    }

    private static void RunSymHeaderCommand(FileInfo sym, FileInfo hdr)
    {
        var file = SymbolFile.Dump(sym.FullName);

        var header = IdaGeneratorUtility.GenerateHeader(file);

        File.WriteAllText(hdr.FullName, header);
    }

    #endregion

    #endregion
}