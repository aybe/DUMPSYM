using System.Security.Cryptography;
using System.Text;

// ReSharper disable CommentTypo
// ReSharper disable InvertIf
// ReSharper disable StringLiteralTypo

namespace DUMPSYM.Tests;

[TestClass]
public sealed class UnitTestY : UnitTestBase
{
    [TestMethod]
    public void TestMethodY()
    {
        var generator = new HeaderGenerator();

        var symbols = Sample.Default.Symbols.ToList();

        // symbols = Generator.CleanupSymbols(symbols); // TODO very slow

        var split = Symbol.Split(symbols.ToArray()).ToList();

        Console.WriteLine($"{split.Count} symbols found");
        
        var remove1 = split.RemoveAll(s => s[0].IsExternal);
        var remove2 = split.RemoveAll(s => s[0].IsFile);
        var remove3 = split.RemoveAll(s => s[0].IsFileEnd);
        var remove4 = split.RemoveAll(s => s[0].IsFunction);
        var remove5 = split.RemoveAll(s => s[0].IsStatic);
        var remove6 = split.RemoveAll(s => s[0].IsVariable);

        Console.WriteLine($"Removed {remove1} externals");
        Console.WriteLine($"Removed {remove2} files");
        Console.WriteLine($"Removed {remove3} file endings");
        Console.WriteLine($"Removed {remove4} functions");
        Console.WriteLine($"Removed {remove5} statics");
        Console.WriteLine($"Removed {remove6} variables");

        Console.WriteLine($"{split.Count} symbols remaining");

        var map = new SortedDictionary<int, Symbol[]>();

        var set = new HashSet<Symbol[]>(SymbolArrayEqualityComparer.MembersTypeName);

        foreach (var item in split)
        {
            if (set.Add(item))
            {
                map.Add(map.Count, item);
            }
        }

        Console.WriteLine($"{set.Count} unique typedefs/types found");

        var filtered = map.Values.ToArray();

        // symbols shall not contain functions else they get visited and yield wrong types/typedefs:
        //
        // member being generated:
        // 15acc4: $00000000 96 Def2 class MOU type STRUCT size 2 dims 0 tag .109fake name WR
        //
        // symbol found in function above it:
        // 14b41b: $00000000 96 Def2 class TPDEF type STRUCT size 3 dims 0 tag .109fake name Palette
        //
        // since lookup is by tag, output becomes wrong; ignoring functions is the right solution

        var original = filtered.SelectMany(s => s).ToList();

        generator.Generate(filtered, original); // TODO same thing passed twice

        const string path = @"C:\Files\GitHub\DUMPSYM\MAIN.SYM.H";

        var contents = generator.Writer.InnerWriter.ToString()!;

        File.WriteAllText(path, contents);

        var hash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(contents)));

        Assert.AreEqual("7c728371592fc8f1d0f50471123556ced99732d39b4c2ed59ee2a49e59bea9c0", hash, true);
    }
}