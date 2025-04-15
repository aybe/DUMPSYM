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
    private static string TestDataPath { get; } = Path.Combine(Solution.Directory, "test_data");

    [ClassInitialize]
    public static void ClassInitialize(TestContext context) // very slow
    {
        if (Directory.Exists(TestDataPath))
        {
            return;
        }

        Console.WriteLine($"Generating test data in '{TestDataPath}'...");

        Directory.CreateDirectory(TestDataPath);

        var input = UnitTest1.GetSample();

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

            var path = Path.Combine(TestDataPath, $"{Path.GetFileName(name)}.json");

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

        Remove(symbols2, structs);

        var unions = SymbolExtensions.GetUnions(symbols2);

        Remove(symbols2, unions);

        var typedefs = SymbolExtensions.GetTypedefs(symbols2);

        Remove(symbols2, typedefs);

        var externals = SymbolExtensions.GetExternals(symbols2);

        Remove(symbols2, externals);

        var statics = SymbolExtensions.GetStatics(symbols2);

        Remove(symbols2, statics);

        var functions = SymbolExtensions.GetFunctions(symbols2);

        Remove(symbols2, functions);

        var files = SymbolExtensions.GetFiles(symbols2);

        Remove(symbols2, files);

        var filesOrphans = SymbolExtensions.GetFilesOrphans(symbols2);

        Remove(symbols2, filesOrphans);

        var names = SymbolExtensions.GetVariables(symbols2);

        Remove(symbols2, names);

        Assert.AreEqual(0, symbols2.Count);
    }

    private static void Remove<T>(List<T> list, List<T> items)
    {
        items.ForEach(s => list.Remove(s));
    }

    private static void Remove<T>(List<T> list, List<List<T>> items)
    {
        items.ForEach(s => Remove(list, s));
    }

    public static IEnumerable<object[]> TestSldFileData()
    {
        return Directory.GetFiles(TestDataPath, "*.json").Select(s => (object[]) [s]);
    }

    public static string TestSldFileName(MethodInfo info, object[] data)
    {
        return $"{info.Name} {Path.GetFileNameWithoutExtension((string)data[0])}";
    }
}