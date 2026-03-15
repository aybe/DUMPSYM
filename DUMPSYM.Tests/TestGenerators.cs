using System.Text.RegularExpressions;
using DUMPSYM.Generators;
using DUMPSYM.Symbols;
using JetBrains.Annotations;

// ReSharper disable StringLiteralTypo

namespace DUMPSYM.Tests;

[TestClass]
[UsedImplicitly]
public sealed class TestGenerators : TestBase
{
    #region New region

    [TestMethod]
    [DynamicData(nameof(GetTestData2), DynamicDataDisplayName = nameof(GetTestName), DynamicDataDisplayNameDeclaringType = typeof(TestBase))]
    public void TestGenerateFiles(string sourcePath, string targetPath)
    {
        // TODO globals

        var lines = File.ReadAllLines(sourcePath).ToList();

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

        var sf = GetSymbolFile();

        var symbols = sf.ToList();

        Directory.CreateDirectory(targetPath);

        var funcs = sf.Symbols.Where(s => s.Record is ISymbolFunction).ToDictionary(s => (ISymbolFunction)s.Record, s => s.Header);

        foreach (var start in symbols.OfType<ISymbolFileStart>())
        {
            var source = Path.Combine(targetPath, Path.GetFileName(start.File));
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

    #endregion

    #region New region

    [TestMethod]
    [UsedImplicitly]
    [DynamicData(nameof(GetTestData1), DynamicDataDisplayName = nameof(GetTestName), DynamicDataDisplayNameDeclaringType = typeof(TestBase))]
    public void TestGenerateHeader(string sourcePath, string targetPath)
    {
        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException(null, sourcePath);
        }

        var file = SymbolFile.Dump(sourcePath);

        var generate = IdaGeneratorUtility.GenerateHeader(file);

        WriteLine(generate);

        Directory.CreateDirectory(targetPath);

        var path = Path.Combine(targetPath, Path.ChangeExtension(Path.GetFileNameWithoutExtension(sourcePath), ".H"));

        File.WriteAllText(path, generate);
    }

    [TestMethod]
    [UsedImplicitly]
    [DynamicData(nameof(GetTestData1), DynamicDataDisplayName = nameof(GetTestName), DynamicDataDisplayNameDeclaringType = typeof(TestBase))]
    public void TestGenerateScript(string sourcePath, string targetPath)
    {
        var file = SymbolFile.Dump(sourcePath);

        IdaGeneratorUtility.GenerateScripts(file, targetPath);
    }

    #endregion
}