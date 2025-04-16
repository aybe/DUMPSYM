using System.Diagnostics;
using System.Reflection;
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

    [TestMethod]
    [DynamicData(nameof(TestSldFileData), DynamicDataDisplayName = nameof(TestSldFileName))]
    public void TestSldFile(string path)
    {
        var symbols1 = SymbolUtility.DeserializeList(File.ReadAllText(path));

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

        registry.Parse();
    }

    public static IEnumerable<object[]> TestSldFileData()
    {
        return Directory.GetFiles(Constants.TestDataDirectory, "*.json").Select(s => (object[]) [s]);
    }

    public static string TestSldFileName(MethodInfo info, object[] data)
    {
        return $"{info.Name} {Path.GetFileNameWithoutExtension((string)data[0])}";
    }
}