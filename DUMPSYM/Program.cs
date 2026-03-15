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
        var root = new Command("scripts") { Description = "Generate IDA scripts from .SYM file" };

        var symArg = new Argument<FileInfo>("source_sym") { Description = "Source .SYM file" };

        var dirArg = new Argument<DirectoryInfo>("target_dir") { Description = "Target directory" };

        root.Add(symArg);

        root.Add(dirArg);

        root.SetAction(result =>
        {
            var sym = result.GetRequiredValue(symArg);

            var dir = result.GetRequiredValue(dirArg);

            RunIdaScriptsCommand(sym, dir);
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

        cmd.Add(GetSymHeaderCommand());

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

    private static Command GetSymHeaderCommand()
    {
        // TODO symbol cleaner options

        var cmd = new Command("header") { Description = "Generate .H file from .SYM file" };

        var symArg = new Argument<FileInfo>("source.sym") { Description = "Source .SYM file" };

        var hdrArg = new Argument<FileInfo>("target.h") { Description = "Target .H file" };

        symArg.Validators.Add(_ => symArg.AcceptExistingOnly());

        symArg.Validators.Add(_ => symArg.AcceptLegalFileNamesOnly());

        hdrArg.Validators.Add(_ => hdrArg.AcceptLegalFileNamesOnly());

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

    private static void RunSymDumpCommand(FileInfo sym)
    {
        using var stream = sym.OpenRead();

        var file = SymbolFile.Dump(stream);

        Console.WriteLine(file.ToString());
    }

    private static void RunSymHeaderCommand(FileInfo sym, FileInfo hdr)
    {
        var file = SymbolFile.Dump(sym.FullName);

        var header = IdaGeneratorUtility.GenerateHeader(file);

        File.WriteAllText(hdr.FullName, header);
    }

    #endregion
}