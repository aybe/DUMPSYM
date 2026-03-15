using System.Text;
using System.Text.RegularExpressions;
using DUMPSYM.Symbols;

// ReSharper disable StringLiteralTypo

namespace DUMPSYM.Generators;

public static class IdaOutputUtility
{
    // TODO refactor params

    public static void SplitFiles(SymbolFile symbolFile, string sourceCode, string targetDirectory)
    {
        var text = File.ReadAllText(sourceCode);

        var sources = SplitFiles(symbolFile, text);

        Directory.CreateDirectory(targetDirectory);

        foreach (var source in sources)
        {
            File.WriteAllText(Path.Combine(targetDirectory, source.Path), source.Text);
        }
    }

    private static List<Source> SplitFiles(SymbolFile symbolFile, string text)
    {
        // TODO globals

        var lines = text.Split(["\r\n", "\n"], StringSplitOptions.None).ToList();

        var functionDeclarations = GetIdaOutputChunk(lines, "// Function declarations");

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

        var dd = GetIdaOutputChunk(lines, "// Data declarations");

        var fn = GetIdaOutputFunctions(lines);

        var symbols = symbolFile.ToList();

        var funcs = symbolFile.Symbols.Where(s => s.Record is ISymbolFunction).ToDictionary(s => (ISymbolFunction)s.Record, s => s.Header);

        var sources = new List<Source>();

        foreach (var start in symbols.OfType<ISymbolFileStart>())
        {
            var source = Path.GetFileName(start.File);
            var header = Path.ChangeExtension(source, ".H");

            var sourceWriter = new StringBuilder();
            var headerWriter = new StringBuilder();

            var pairs = funcs.Where(s => s.Key.File == start.File);

            foreach (var pair in pairs)
            {
                var key = pair.Value.Address.ToString("X8");

                if (fn.TryGetValue(key, out var value))
                {
                    sourceWriter.AppendLine($"//----- ({key}) --------------------------------------------------------");
                    sourceWriter.AppendLine((string?)value);
                    sourceWriter.AppendLine();

                    if (functionDeclarationsMap.TryGetValue(pair.Key.Name, out var declaration))
                    {
                        headerWriter.AppendLine(declaration);
                    }
                    else
                    {
                        Console.WriteLine($"WARNING: function declaration not found for {pair.Key.Name}");
                        // _card_event, _clear_event, _card_event_x, _clear_event_x, __cmpsf2, __cmpdf2
                    }
                }
                else
                {
                    sourceWriter.AppendLine($"#error \"function {key} was in symbols but not in IDA output\"");
                }
            }

            sources.Add(new Source(source, sourceWriter.ToString()));
            sources.Add(new Source(header, headerWriter.ToString()));
        }

        return sources;
    }

    private static List<string> GetIdaOutputChunk(List<string> lines, string header)
    {
        var index1 = lines.FindIndex(s => s == header);

        index1 += 2;

        var index2 = lines.FindIndex(index1, s => s == string.Empty);

        var output = lines[index1..index2];

        lines.RemoveRange(index1, index2 - index1);

        return output;
    }

    private static Dictionary<string, string> GetIdaOutputFunctions(List<string> lines)
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