using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
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
        var printTypes = false;
        var printTypedefs = false;

        var types = Factory.GetDistinctTypes();

        if (printTypes)
        {
            WriteLineVar(types.Length);

            foreach (var symbols in types)
            {
                WriteLine(symbols[0]);
            }

            WriteLine();
        }

        var typedefs = Factory.GetDistinctTypeDefinitions();

        if (printTypedefs)
        {
            WriteLineVar(typedefs.Length);

            foreach (var symbols in typedefs)
            {
                WriteLine(symbols[0]);
            }
        }

        var showDuplicates = true;
        var showUniques = true;

        var lookup = types
            .ToLookup(s => s[0].Name!)
            .Where(s => (showDuplicates && s.Count() > 1) || (showUniques && s.Count() == 1))
            .ToArray();

        var mapType2TypeDefinition = types.ToFrozenDictionary(s => s, s => Factory.GetTypeDefinition(s[^1]));
        var compilerGenerated = mapType2TypeDefinition.Where(s => SymbolFactory.HasFakeName(s.Key[0].Name!) && s.Value == null).Select(s => s.Key).ToArray();
        var compilerGeneratedStructurallySame = compilerGenerated.GroupBy(s => s, SymbolArrayEqualityComparer.Members).Where(s => s.Count() > 1).ToArray();

        foreach (var grouping in compilerGeneratedStructurallySame)
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

                var eos = symbols[^1];

                WriteLine($"\t{hdr}");

                var name = Factory.GetTypeName(hdr, eos, out var def);

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
}