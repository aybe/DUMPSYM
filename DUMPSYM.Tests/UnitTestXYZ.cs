// #define LOG

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using DUMPSYM.Extensions;

namespace DUMPSYM.Tests;

[TestClass]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed class UnitTestXYZ : UnitTestBase
{
    [TestMethod]
    public void TestConcatenateSymbols()
    {
        // names can appear at any time in the file and are not ordered by their address
        // externals have the same address and name in addition to the type of variable
        // this doesn't solve the problem of where a variable is initially declared...

        const bool removeExternals = true;
        const bool removeFiles = false;
        const bool removeFilesEndings = false;
        const bool removeFunctions = true;
        const bool removeNames = true;
        const bool removeStatics = true;
        const bool removeDuplicateGroups = true;

        var split = Symbol.Split(Sample.Default.Symbols.ToArray()).ToList();

        WriteLineVar(split.Count);

        Remove(s => s.First().Record.IsExternal(), removeExternals, "Removing externals...");

        Remove(s => s.First().Record.IsFileHeader(), removeFiles, "Removing files...");

        Remove(s => s.First().Record.IsFileEnd(), removeFilesEndings, "Removing files endings...");

        Remove(s => s.First().Record.IsFunctionHeader(), removeFunctions, "Removing functions...");

        Remove(s => s.First().Record.IsName(), removeNames, "Removing names...");

        Remove(s => s.First().Record.IsStatic(), removeStatics, "Removing statics...");

        const bool fixFakesBefore = true;

        if (fixFakesBefore)
        {
            WriteLine("Fixing fakes...");
            FixFakes(split);
        }

        WriteLine("Grouping symbols...");

        var groups = split.ToLookup(s => s[0].Record, s => s).ToArray();

        WriteLineVar(groups.Length);

        Assert.AreNotEqual(split.Count, groups.Length);

        if (!fixFakesBefore)
        {
            WriteLine("Fixing fakes...");
            var symbolsList = groups.SelectMany(s => s).ToList();
            symbolsList = groups.Select(s => s.First()).ToList();
            FixFakes(symbolsList);
        }

        if (removeDuplicateGroups)
        {
            WriteLine("Removing duplicate groups...");

            var duplicates = groups.Where(s => s.Count() > 1).ToArray();

            foreach (var grouping in duplicates)
            {
                var removed = split.RemoveAll(s => grouping.Contains(s));

                Assert.AreEqual(grouping.Count(), removed);
            }

            WriteLineVar(split.Count);
        }

        WriteLine($"{groups.Length} groups remaining:");

        // Console.WriteLine(string.Join("\n", groups.Select(s => s.Key.ToString())));

        using var writer = new StringWriter();

        foreach (var grouping in groups)
        {
            writer.WriteLine($"Count = {grouping.Count()}, Key = {grouping.Key}".ReplaceLineEndings(", "));
        }

        var result = writer.ToString();

        File.WriteAllText(@"C:\Files\GitHub\! PSX\DUMPSYM\group by record.txt", result);

        WriteLine(result);

        {
            using var sw = new StringWriter();
            var symbols = groups.SelectMany(s => s.First()).ToList();
            var parse = SymbolParserUtility.Parse(symbols);
            sw.WriteLine("// TODO remove size_t");
            sw.WriteLine("// TODO remove wchar_t");
            sw.WriteLine("// ReSharper disable CppInconsistentNaming");
            sw.WriteLine("// ReSharper disable CommentTypo");
            sw.WriteLine("// ReSharper disable IdentifierTypo");
            sw.WriteLine("// ReSharper disable CppClangTidyClangDiagnosticReservedIdentifier");
            sw.WriteLine("// ReSharper disable CppClangTidyBugproneReservedIdentifier");
            sw.WriteLine(parse);
            //File.WriteAllText(@"C:\Files\GitHub\! PSX\DUMPSYM\Project1\test.cpp", sw.ToString());
        }
        return;

        void Remove(Predicate<Symbol[]> predicate, bool condition, object? message)
        {
            if (!condition)
            {
                return;
            }

            if (message != null)
            {
                WriteLine(message);
            }

            var removed = split.RemoveAll(predicate);

            WriteLineVar(removed);

            WriteLineVar(split.Count);
        }
    }

    [TestMethod]
    public void TestNamesNotSortedByAddress()
    {
        var check = Check(Sample.Default.Records.Where(s => s.Value is ISymbolVariable).Select(s => s.Key));

        Assert.IsFalse(check); // expected, it's garbage

        return;

        static bool Check(IEnumerable<SymbolHeader> headers)
        {
            var last = -1L;

            foreach (var header in headers)
            {
                var address = header.Address;

                if (address <= last)
                {
                    return false;
                }

                last = address;
            }

            return true;
        }
    }

    [SuppressMessage("ReSharper", "InvertIf")]
    [SuppressMessage("ReSharper", "CommentTypo")]
    private static void FixFakes(List<Symbol[]> sym)
    {
        var regexFake = new Regex(@"^\.(\d+)fake$", RegexOptions.Compiled);

        var uniques = new ConcurrentDictionary<string, int>(); // TODO maybe move inside

        var files = sym.Split(s => s[0].Record is SymbolRecordSetSldToLineOfFile);

        foreach (var file in files)
        {
            if (file[0][0].Record.IsFileHeader(out var start))
            {
                switch (Path.GetFileName(start.File))
                {
                    case "STATS.C": // BUG repeats symbols thrice
                        break;
                    case "THING.C": // BUG repeats symbols twice
                        break;
                }

                Console.WriteLine(start.File);
            }

            var fakes = new Dictionary<string, string>();

            var symbols = file.SelectMany(s => s).ToArray();

            {
                var end1 = Array.FindIndex(symbols, 0, s => s.Record.IsFileEnd());

                Assert.AreNotEqual(-1, end1);

                var end2 = Array.FindIndex(symbols, end1 + 1, s => s.Record.IsFileEnd());

                if (end2 != -1)
                {
                    symbols = symbols[..end2];
                }
            }

            for (var i = 0; i < symbols.Length; i++)
            {
                var header = symbols[i].Record;

                if (header.IsType(out var type))
                {
                    var key = type.Name;

                    if (regexFake.IsMatch(key))
                    {
                        fakes.Add(key, ""); // TODO delete when STATS/THING fixed
                        var def = default(ISymbolDefinition2);
                        var idx = Array.FindIndex(symbols, i, s => s.Record.IsTypedef2(out def) && def.Tag == key);

                        var real = idx == -1 ? $"_{key[1..]}_{uniques.AddOrUpdate(key, 0, (_, t) => ++t)}" : def!.Name;
                        Console.WriteLine($"{idx}, {real}, {type}");
                    }
                    else
                    {
                        Console.WriteLine($"0, {key}, {type}");
                    }
                }
            }
        }
    }
}