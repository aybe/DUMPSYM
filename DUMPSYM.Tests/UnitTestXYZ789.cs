using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

// ReSharper disable CommentTypo

namespace DUMPSYM.Tests;

[TestClass]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed class UnitTestXYZ789 : UnitTestBase
{
    private SymbolFactory Factory { get; } = new(Sample.Default);

    [TestMethod]
    public void TestSplitByFiles()
    {
        Assert.AreEqual(311, Factory.DistinctTypes.Length);

        Assert.AreEqual(203, Factory.DistinctTypeDefinitions.Length);

        var printTypes = false;
        var printTypedefs = false;

        if (printTypes)
        {
            Print(Factory.DistinctTypes);
        }

        if (printTypedefs)
        {
            Print(Factory.DistinctTypeDefinitions);
        }

        var showDuplicates = true;
        var showUniques = true;

        var lookup = Factory.DistinctTypes
            .ToLookup(s => s[0].Name!)
            .Where(s => (showDuplicates && s.Count() > 1) || (showUniques && s.Count() == 1))
            .ToArray();

        Assert.AreEqual(300, lookup.Length);

        foreach (var grouping in Factory.CompilerGeneratedTypesDuplicates)
        {
            var name = grouping.Key[0].Name!;

            WriteLine(name);

            foreach (var symbols in grouping)
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

                var name = Factory.GetTypeName(symbols, out var def);

                WriteLine($"\t\t{name}");

                var ln = Factory.LineOf(hdr);

                var c1 = $"[{hdr}]({uri}:{ln})";

                var c2 = def == null ? name : $"[{def.Name}]({uri}:{Factory.LineOf(def)})";

                tmp.Add(ln, $"| {ln} | {c1} | {c2} |");
            }
        }

        foreach (var row in tmp.Values)
        {
            sb.AppendLine(row);
        }

        File.WriteAllText(output, sb.ToString());
    }

    private void Print(Array array, [CallerArgumentExpression(nameof(array))] string arrayName = null!)
    {
        WriteLine($"{arrayName}: {array.Length}");

        foreach (var item in array)
        {
            WriteLine(item is Array a ? a.GetValue(0) : item);
        }
    }
}