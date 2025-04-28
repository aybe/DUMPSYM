using System.Diagnostics.CodeAnalysis;
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
        const bool removeFiles = true;
        const bool removeFilesEndings = true;
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

        WriteLine("Grouping symbols...");

        var groups = split.ToLookup(s => s[0].Record, s => s).ToArray();

        WriteLineVar(groups.Length);

        Assert.AreNotEqual(split.Count, groups.Length);

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

        using var writer = new StringWriter();

        foreach (var grouping in groups)
        {
            writer.WriteLine($"Count = {grouping.Count()}, Key = {grouping.Key}".ReplaceLineEndings(", "));
        }

        var result = writer.ToString();

        File.WriteAllText(@"C:\Files\GitHub\! PSX\DUMPSYM\group by record.txt", result);

        WriteLine(result);

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
}