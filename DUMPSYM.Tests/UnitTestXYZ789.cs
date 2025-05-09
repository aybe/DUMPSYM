using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
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
        Assert.AreEqual(311, Factory.DistinctType.Count);

        Assert.AreEqual(203, Factory.DistinctTypeDefinition.Count);

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

                var name = Factory.DistinctTypeName[symbols];

                var def = Factory.DistinctTypeDefinitionMap[symbols];

                WriteLine($"\t\t{name}");

                var ln = Factory.MapLine[hdr];

                var c1 = $"[{hdr}]({uri}:{ln})";

                var c2 = def == null ? name : $"[{def.Name}]({uri}:{Factory.MapLine[def]})";

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
}