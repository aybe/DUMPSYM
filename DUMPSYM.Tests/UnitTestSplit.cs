using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using DUMPSYM.Extensions;
using Newtonsoft.Json;

namespace DUMPSYM.Tests;

[TestClass]
// wondering why we're using json instead of some typed data?
// you can't pass any type you want, it'll eat up all memory and fail
// live testing runs faster as serializing everything at start is slow
// https://github.com/microsoft/testfx/issues/1767#issuecomment-1794345657
public sealed class UnitTestSplit222 : UnitTestBase
{
    [ClassInitialize]
    public static void ClassInitialize(TestContext context) // very slow
    {
        var directory = Constants.TestDataDirectory;

        if (Directory.Exists(directory))
        {
            return;
        }

        Console.WriteLine($"Generating test data in '{directory}'...");

        Directory.CreateDirectory(directory);

        var input = Sample.Default;

        var lists = input.Symbols.Split(s => s.Record is SymbolRecordSetSldToLineOfFile);

        var total = TimeSpan.Zero;

        foreach (var symbols in lists)
        {
            var stopwatch = Stopwatch.StartNew();

            var json = SymbolUtility.SerializeList(symbols, Formatting.Indented);

            var elapsed = stopwatch.Elapsed;

            Console.WriteLine($"Generating data for test took {elapsed}...");

            total += elapsed;

            var name = ((SymbolRecordSetSldToLineOfFile)symbols[0].Record).File;

            var path = Path.Combine(directory, $"{Path.GetFileName(name)}.json");

            File.WriteAllText(path, json);
        }

        Console.WriteLine($"Generating data for all tests took {total}");
    }

    public static IEnumerable<object[]> GetFileTestJson()
    {
        return Directory.GetFiles(Constants.TestDataDirectory, "*.json").Select(s => (object[]) [s]);
    }

    public static string GetFileTestName(MethodInfo info, object[] data)
    {
        return $"{info.Name} {Path.GetFileNameWithoutExtension((string)data[0])}";
    }

    public static List<Symbol> GetSymbols(string path)
    {
        var symbols = SymbolUtility.DeserializeList(File.ReadAllText(path));

        return symbols;
    }

    public static SymbolRegistry GetSymbolRegistry(string path)
    {
        var symbols1 = GetSymbols(path);

        var symbols2 = symbols1.Select(s => s.Record).Cast<ISymbol>().ToList(); // TODO sucks

        var structs = SymbolExtensions.GetStructs(symbols2);

        symbols2.Remove(structs);

        var unions = SymbolExtensions.GetUnions(symbols2);

        symbols2.Remove(unions);

        var typedefs = SymbolExtensions.GetTypedefs(symbols2);

        symbols2.Remove(typedefs);

        var externals = SymbolExtensions.GetExternals(symbols2);

        symbols2.Remove(externals);

        var statics = SymbolExtensions.GetStatics(symbols2);

        symbols2.Remove(statics);

        var functions = SymbolExtensions.GetFunctions(symbols2);

        symbols2.Remove(functions);

        var files = SymbolExtensions.GetFiles(symbols2);

        symbols2.Remove(files);

        var filesOrphans = SymbolExtensions.GetFilesOrphans(symbols2);

        symbols2.Remove(filesOrphans);

        var names = SymbolExtensions.GetVariables(symbols2);

        symbols2.Remove(names);

        Assert.AreEqual(0, symbols2.Count, string.Join(Environment.NewLine, symbols2));

        var registry = new SymbolRegistry
        {
            Externals = externals,
            Files = files,
            Functions = functions,
            Names = names,
            Statics = statics,
            Structs = structs,
            Typedefs = typedefs,
            Unions = unions
        };

        return registry;
    }

    /// <summary>
    ///     Parses symbols manually.
    /// </summary>
    /// <param name="path"></param>
    [TestMethod]
    [DynamicData(nameof(GetFileTestJson), DynamicDataDisplayName = nameof(GetFileTestName))]
    public void TestParsingUsingCustomLogic(string path)
    {
        var registry = GetSymbolRegistry(path);

        registry.Parse();
    }

    /// <summary>
    ///     Parses symbols in file order.
    /// </summary>
    /// <param name="path"></param>
    [TestMethod]
    [DynamicData(nameof(GetFileTestJson), DynamicDataDisplayName = nameof(GetFileTestName))]
    public void TestParsingUsingFileOrder(string path)
    {
        var symbols = GetSymbols(path);

        var parse = SymbolParserUtility.Parse(symbols);

        WriteLine(parse);

        Assert.IsTrue(string.IsNullOrWhiteSpace(parse));
    }

    /// <summary>
    ///     Sorts symbols using a comparer.
    /// </summary>
    /// <param name="path"></param>
    [TestMethod]
    [DynamicData(nameof(GetFileTestJson), DynamicDataDisplayName = nameof(GetFileTestName))]
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public void TestSortingUsingCustomComparer(string path)
    {
        var registry = GetSymbolRegistry(path);

        var source = registry.CreateLists();

        var comparer = CodeComparer.Instance;

        comparer.Count = 0;
        var target = source.Order(comparer).ToList();

        WriteLineVar(comparer.Count);
        WriteLineVar(source.Count);
        WriteLineVar(source.Sum(s => s.Count));

        var verbose = false;

        var output = verbose
            ? string.Join(Environment.NewLine, target.SelectMany(s => s.Symbols))
            : string.Join(Environment.NewLine, target.Select(s => s.Symbols[0].ToString()?.ReplaceLineEndings(" ")));

        string filter;
        filter = "STRTAG";
        filter = null!;

        output = $"{DateTime.Now:O}{Environment.NewLine}{output}";

        if (string.IsNullOrWhiteSpace(filter))
        {
            WriteLine(output);
        }
        else
        {
            using var reader = new StringReader(output);

            while (true)
            {
                var line = reader.ReadLine();

                if (line == null)
                {
                    break;
                }

                if (line.Contains(filter))
                {
                    WriteLine(line);
                }
            }
        }

        string combine;
        // bug not working even with .runsettings
        // https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-configure
        combine = Path.Combine(TestContext.TestRunResultsDirectory!, $"sort for {Path.GetFileName(path)}.txt");
        combine = Path.Combine(TestContext.TestResultsDirectory!, $"sort for {Path.GetFileName(path)}.txt");
        combine = Path.Combine(TestContext.TestRunDirectory!, $"sort for {Path.GetFileName(path)}.txt");
        combine = Path.ChangeExtension(path, ".result");
        File.WriteAllText(combine, output);
        TestContext.AddResultFile(combine);
        if (false)
        if (path is @"C:\Files\GitHub\! PSX\DUMPSYM\TestData\AFFECT.C.json")
        {
            Assert.IsTrue(
                output.IndexOf("Def class STRTAG type STRUCT size 32 name .1fake", StringComparison.Ordinal) <
                output.IndexOf("Def2 class TPDEF type STRUCT size 32 dims 0 tag .1fake name MATRIX", StringComparison.Ordinal)
            );
        }
    }

    /// <summary>
    ///     <see cref="SymbolExtensions" /> methods must find every symbol.
    /// </summary>
    [TestMethod]
    [DynamicData(nameof(GetFileTestJson), DynamicDataDisplayName = nameof(GetFileTestName))]
    public void TestSymbolSearch(string path)
    {
        var registry = GetSymbolRegistry(path);
    }
}