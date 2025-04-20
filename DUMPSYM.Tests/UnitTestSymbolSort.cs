using System.Diagnostics.CodeAnalysis;

// ReSharper disable StringLiteralTypo
// ReSharper disable ArrangeTrailingCommaInMultilineLists
// ReSharper disable CommentTypo

namespace DUMPSYM.Tests;

[TestClass]
public sealed class UnitTestSymbolSort : UnitTestBase
{
    [TestMethod]
    public void TopologicalSort()
    {
        // TODO try passing them in original order to preserve initial order, this will require symbol header maybe
        var lists = UnitTestSplit222
            .GetSymbolRegistry(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\AFFECT.C.json")
            .CreateLists();
        var symbols = lists
            .Select(s => new Sym { Code = s }).ToList();

        symbols.AddRange([
            new Sym { Code = new Code(new SymbolRecordDef { Class = SymbolStorageClass.TPDEF, Type = new SymbolType { Value = 0 }, Name = "NULL" }) },
            new Sym { Code = new Code(new SymbolRecordDef { Class = SymbolStorageClass.TPDEF, Type = new SymbolType { Value = 1 }, Name = "VOID" }) },
            new Sym { Code = new Code(new SymbolRecordDef { Class = SymbolStorageClass.TPDEF, Type = new SymbolType { Value = 2 }, Name = "CHAR" }) },
            new Sym { Code = new Code(new SymbolRecordDef { Class = SymbolStorageClass.TPDEF, Type = new SymbolType { Value = 3 }, Name = "SHORT" }) },
            new Sym { Code = new Code(new SymbolRecordDef { Class = SymbolStorageClass.TPDEF, Type = new SymbolType { Value = 4 }, Name = "INT" }) },
            new Sym { Code = new Code(new SymbolRecordDef { Class = SymbolStorageClass.TPDEF, Type = new SymbolType { Value = 5 }, Name = "LONG" }) },
            new Sym { Code = new Code(new SymbolRecordDef { Class = SymbolStorageClass.TPDEF, Type = new SymbolType { Value = 6 }, Name = "FLOAT" }) },
            new Sym { Code = new Code(new SymbolRecordDef { Class = SymbolStorageClass.TPDEF, Type = new SymbolType { Value = 7 }, Name = "DOUBLE" }) }, // none
            new Sym { Code = new Code(new SymbolRecordDef { Class = SymbolStorageClass.TPDEF, Type = new SymbolType { Value = 12 }, Name = "UCHAR" }) },
            new Sym { Code = new Code(new SymbolRecordDef { Class = SymbolStorageClass.TPDEF, Type = new SymbolType { Value = 13 }, Name = "USHORT" }) },
            new Sym { Code = new Code(new SymbolRecordDef { Class = SymbolStorageClass.TPDEF, Type = new SymbolType { Value = 14 }, Name = "UINT" }) }
            //new Sym { Code = new Code(new SymbolRecordDef { Class = SymbolStorageClass.TPDEF, Type = new SymbolType { Value = 15 }, Name = "ULONG" }) }, // already
        ]);
/*
   1fab97: $00000000 94 Def class STRTAG type STRUCT size 8 name Proportion
   1fabaf: $00000000 94 Def class MOS type LONG size 0 name angle
   1fabc2: $00000004 94 Def class MOS type LONG size 0 name hypotenuse
   1fabda: $00000008 96 Def2 class EOS type NULL size 8 dims 0 tag Proportion name .eos
 */

        var sym1 = new Sym
        {
            Code = new Code([
                new SymbolRecordDef { Class = SymbolStorageClass.STRTAG, Type = new SymbolType { Value = 8 }, Name = "NCB" },
                new SymbolRecordDef { Class = SymbolStorageClass.MOS, Type = new SymbolType { Value = 4 }, Name = "BAD_SYMBOL" },
                new SymbolRecordDef2 { Class = SymbolStorageClass.EOS, Type = new SymbolType { Value = 0 }, Tag = "NCB", Name = ".eos" }
            ])
        };

        symbols.Add(sym1);

        foreach (var sym in symbols)
        {
            sym.ResolveDependencies();
        }

        //Console.WriteLine(symbols.RemoveAll(s => s.Name == new SymKey(SymbolStorageClass.STRTAG, "SpriteData")));

        var sort = TopologicalSort(symbols, out var result);

        foreach (var sym in result!)
        {
            Console.WriteLine(sym);
        }

        Assert.IsTrue(sort, "Topological sort failed.");
    }

    private static bool TopologicalSort(List<Sym> symbols, [MaybeNullWhen(false)] out List<Sym> result)
    {
        result = null;

        var symbolMap = symbols.ToDictionary(s => s.Name, s => s);

        var inDegree = symbols.ToDictionary(s => s.Name, _ => 0);

        var graph = new Dictionary<SymKey, List<SymKey>>();

        foreach (var symbol in symbols) // build the graph and in-degree count
        {
            foreach (var dependency in symbol.Dependencies)
            {
                if (!graph.TryGetValue(dependency, out var list))
                {
                    list = graph[dependency] = [];
                }

                var key = symbol.Name;

                list.Add(key);

                inDegree[key]++;
            }
        }

        Console.WriteLine($"{nameof(symbolMap)}: {symbolMap.Count}");
        Console.WriteLine($"{nameof(inDegree)}: {inDegree.Count}");
        Console.WriteLine($"{nameof(graph)}: {graph.Count}");

        // queue for symbols with no dependencies

        var queue = new Queue<SymKey>(inDegree.Where(s => s.Value == 0).Select(kv => kv.Key));

        Console.WriteLine($"{nameof(queue)}: {queue.Count}");

        var sorted = new List<Sym>();

        while (queue.Count > 0)
        {
            var key = queue.Dequeue();

            sorted.Add(symbolMap[key]);

            if (!graph.TryGetValue(key, out var neighbors))
            {
                continue;
            }

            foreach (var neighbor in neighbors)
            {
                inDegree[neighbor]--;

                if (inDegree[neighbor] == 0)
                {
                    queue.Enqueue(neighbor);
                }
            }
        }

        Console.WriteLine($"{nameof(sorted)}: {sorted.Count}");

        // Check for cycles
        if (sorted.Count != symbols.Count)
        {
            var list = symbols.Where(x => !sorted.Contains(x)).ToList();
            result = list;

            return false;
        }

        result = sorted;

        return true;
    }
}