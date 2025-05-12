using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace DUMPSYM.Tests;

[TestClass]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed class UnitTestXYZ789 : UnitTestBase
{
    private SymbolFactory Factory { get; } = new(Sample.Default);

    [TestInitialize]
    public void TestInitialize()
    {
        Assert.AreEqual(311, Factory.DistinctType.Count);

        Assert.AreEqual(203, Factory.DistinctTypeDefinition.Count);
    }

    [TestMethod]
    public void TestGeneration()
    {
        var generator = new SymbolGenerator(Factory);

        var s = generator.ToString();

        WriteLine(s);

        const string path = @"C:\Files\GitHub\! PSX\DUMPSYM\MAIN.SYM.H"; // TODO get from file

        File.WriteAllText(path, s);
    }

    [TestMethod]
    public void TestGroupTypeByFile()
    {
        var files = new SymbolFactoryFiles(Sample.Default);

        var set = Factory.DistinctType;

        WriteLineVar(set.Count);

        var lookup = set.ToLookup(s => files.Reverse[s.First()]);

        foreach (var grouping in lookup)
        {
            WriteLine($"File: {grouping.Key.File}, Count: {grouping.Count()}");
        }
    }

    [TestMethod]
    public void TestGroupTypeDefinitionByFile()
    {
        var files = new SymbolFactoryFiles(Sample.Default);

        var set = Factory.DistinctTypeDefinition;

        WriteLineVar(set.Count);

        var lookup = set.ToLookup(s => files.Reverse[s]);

        foreach (var grouping in lookup)
        {
            WriteLine($"File: {grouping.Key.File}, Count: {grouping.Count()}");
        }
    }

    [TestMethod]
    public void TestSplitByFiles()
    {
        var printTypes = false;
        var printTypedefs = false;

        if (printTypes)
        {
            WriteLineVar(Factory.DistinctType.Count);

            foreach (var symbols in Factory.DistinctType)
            {
                WriteLine(symbols[0]);
            }
        }

        if (printTypedefs)
        {
            WriteLineVar(Factory.DistinctTypeDefinition.Count);

            foreach (var symbol in Factory.DistinctTypeDefinition)
            {
                WriteLine(symbol);
            }
        }

        var showDuplicates = true;
        var showUniques = true;

        var lookup = Factory.DistinctType
            .ToLookup(s => s[0].Name!)
            .Where(s => (showDuplicates && s.Count() > 1) || (showUniques && s.Count() == 1))
            .ToArray();

        Assert.AreEqual(300, lookup.Length);

        foreach (var (key, set) in Factory.GeneratedTypeGroup)
        {
            var name = key[0].Name!;

            WriteLine(name);

            foreach (var symbols in set)
            {
                WriteLine("\t" + symbols[0]);
            }
        }

        const string path = @"C:\Files\GitHub\! PSX\DUMPSYM\MAIN.SYM.txt"; // TODO as parameter

        var output = Path.ChangeExtension(path, "md");

        WriteLine(new Uri(output).AbsoluteUri);

        WriteLine($"{lookup.Length} types with resolved names, {nameof(showDuplicates)} = {showDuplicates}, {nameof(showUniques)} = {showUniques}:");

        var sb = new StringBuilder();

        sb.AppendLine("| Line | Type | Name |");
        sb.AppendLine("|------|------|------|");

        var uri = new Uri(path).AbsoluteUri.Replace("file:///", "vscode://file/");

        var tmp = new SortedDictionary<int, string>();

        foreach (var group in lookup)
        {
            WriteLine($"{group.Key} ({group.Count()})");

            foreach (var symbols in group)
            {
                var hdr = symbols[0];

                WriteLine($"\t{hdr}");

                var name = Factory.DistinctTypeName[symbols];

                var def = Factory.DistinctTypeDefinitionMap[symbols];

                WriteLine($"\t\t{name}");

                var ln = Factory.LineOf[hdr];

                var c1 = $"[{hdr}]({uri}:{ln})";

                var c2 = def == null ? name : $"[{def.Name}]({uri}:{Factory.LineOf[def]})";

                tmp.Add(ln, $"| {ln} | {c1} | {c2} |");
            }
        }

        foreach (var row in tmp.Values)
        {
            sb.AppendLine(row);
        }

        var md = sb.ToString();

        var hash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(md)));

        Assert.AreEqual("a461f9c42453d799deb60ddfce115de4b456ac46c0134ff371fd8e67593593be", hash, true);

        File.WriteAllText(output, md);
    }

    [TestMethod]
    public void TestSymbolStorageClassOccurrences()
    {
        var symbols = Sample.Default.Symbols;

        var symbolsCount = symbols.Count;

        WriteLineVar(symbolsCount);

        var found = 0;

        foreach (var klass in Enum.GetValues<SymbolStorageClass>())
        {
            var count = symbols.Count(s => s.Class == klass);

            WriteLine($"{klass}: {count}");

            found += count;
        }

        WriteLine($"{nameof(found)}: {found} ({(float)found / symbolsCount:P})");

        var missing = symbols.Count(s => s.Class == null);

        WriteLine($"{nameof(missing)}: {missing} ({(float)missing / symbolsCount:P})");

#pragma warning disable CS0162 // Unreachable code detected
        // ReSharper disable HeuristicUnreachableCode
        switch (new SymbolStorageClass())
        {
            case SymbolStorageClass.AUTO: // appear in functions
                break;
            case SymbolStorageClass.EXT: // externals
                break;
            case SymbolStorageClass.STAT: // statics
                break;
            case SymbolStorageClass.REG: // appear in functions
                break;
            case SymbolStorageClass.LABEL: // appear in functions
                break;
            case SymbolStorageClass.MOS: // struct member
                break;
            case SymbolStorageClass.ARG: // appear in functions
                break;
            case SymbolStorageClass.STRTAG: // struct
                break;
            case SymbolStorageClass.MOU: // union member
                break;
            case SymbolStorageClass.UNTAG: // union
                break;
            case SymbolStorageClass.TPDEF: // typedef
                break;
            case SymbolStorageClass.ENTAG: // enum
                break;
            case SymbolStorageClass.MOE: // enum member
                break;
            case SymbolStorageClass.REGPARM: // appear in functions
                break;
            case SymbolStorageClass.FIELD: // bit field
                break;
            case SymbolStorageClass.EOS: // end of struct/union/enum
                break;
            case SymbolStorageClass.FILE: // files
                break;
            // ReSharper disable once RedundantEmptySwitchSection
            default:
                break;
        }
        // ReSharper restore HeuristicUnreachableCode
#pragma warning restore CS0162 // Unreachable code detected
    }
}