using System.Text.RegularExpressions;
using DUMPSYM.Symbols;

// ReSharper disable CommentTypo

// ReSharper disable StringLiteralTypo

namespace DUMPSYM.Tests;

[TestClass]
public sealed class UnitTestIdaSplit : UnitTestBase
{
    [TestMethod]
    public void Test()
    {
        // TODO globals

        var lines = File.ReadAllLines(@"C:\GitHub\HigherOctane\TEMP\001-only-types-and-functions-applied.c").ToList();

        var functionDeclarations = GetChunk(lines, "// Function declarations");

        var functionDeclarationRegex = new Regex(@"(\w+)\(");

        var functionDeclarationsMap = new Dictionary<string, string>();

        foreach (var input in functionDeclarations)
        {
            var match = functionDeclarationRegex.Match(input);

            var value = match.Groups["1"].Value;

            if (!functionDeclarationsMap.TryAdd(value, input))
            {
                Console.WriteLine($"ERROR: function declaration already in map: {value}"); // BUG: only one -> SpuSetVoiceAttr
            }
        }

        var dd = GetChunk(lines, "// Data declarations");

        var fn = GetFunctions(lines);

        var sf = GetSymbolFile();

        var symbols = sf.ToList();

        const string output = @"C:\GitHub\HigherOctane\TEMP\OUTPUT";

        Directory.CreateDirectory(output);

        var funcs = sf.Symbols.Where(s => s.Record is ISymbolFunction).ToDictionary(s => (ISymbolFunction)s.Record, s => s.Header);

        foreach (var start in symbols.OfType<ISymbolFileStart>())
        {
            var source = Path.Combine(output, Path.GetFileName(start.File));
            var header = Path.ChangeExtension(source, ".H");

            using var sourceWriter = File.CreateText(source);
            using var headerWriter = File.CreateText(header);

            var pairs = funcs.Where(s => s.Key.File == start.File);

            foreach (var pair in pairs)
            {
                var key = pair.Value.Address.ToString("X8");

                if (fn.TryGetValue(key, out var value))
                {
                    sourceWriter.WriteLine($"//----- ({key}) --------------------------------------------------------");
                    sourceWriter.WriteLine(value);
                    sourceWriter.WriteLine();

                    if (functionDeclarationsMap.TryGetValue(pair.Key.Name, out var declaration))
                    {
                        headerWriter.WriteLine(declaration);
                    }
                    else
                    {
                        Console.WriteLine($"WARNING: function declaration not found for {pair.Key.Name}");
                        // _card_event, _clear_event, _card_event_x, _clear_event_x, __cmpsf2, __cmpdf2
                    }
                }
                else
                {
                    sourceWriter.WriteLine($"#error \"function {key} was in symbols but not in IDA output\"");
                }
            }
        }
    }

    private static SymbolFile GetSymbolFile(string path = @"C:\GitHub\DUMPSYM\MAIN.SYM")
    {
        using var stream = File.OpenRead(path);

        var file = SymbolFile.Dump(stream);

        return file;
    }

    private static List<string> GetChunk(List<string> lines, string header)
    {
        var index1 = lines.FindIndex(s => s == header);

        index1 += 2;

        var index2 = lines.FindIndex(index1, s => s == string.Empty);

        var output = lines[index1..index2];

        lines.RemoveRange(index1, index2 - index1);

        return output;
    }

    private static Dictionary<string, string> GetFunctions(List<string> lines)
    {
        var functions = new Dictionary<string, string>();

        while (true)
        {
            const string prefix = "//----- (";

            var index1 = lines.FindIndex(s => s.StartsWith(prefix));

            if (index1 == -1)
            {
                break;
            }

            var index2 = lines.FindIndex(index1 + 1, s => s.StartsWith(prefix));

            if (index2 == -1)
            {
                index2 = lines.FindIndex(index1 + 1, s => s.StartsWith("// nfuncs="));
            }

            var source = lines[(index1 + 1)..(index2 - 1)];

            var offset = lines[index1].Substring(prefix.Length, 8);

            functions.Add(offset, string.Join(Environment.NewLine, source));

            lines.RemoveRange(index1, index2 - index1);
        }

        return functions;
    }
}