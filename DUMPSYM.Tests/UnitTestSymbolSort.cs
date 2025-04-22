using System.Diagnostics.CodeAnalysis;

// ReSharper disable StringLiteralTypo
// ReSharper disable ArrangeTrailingCommaInMultilineLists
// ReSharper disable CommentTypo

namespace DUMPSYM.Tests;

[TestClass]
public sealed class UnitTestSymbolSort : UnitTestBase
{
    [TestMethod] // TODO dynamic data
    // ReSharper disable StringLiteralTypo
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\AFFECT.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\ANGLE.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\BUILDING.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\CAMERA.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\CONTROL.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\DISTANCE.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\DRAW.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\EFFECT.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\ENGINE.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\FLOATLIB.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\FORLIB.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\GAME.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\GENMAP.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\LEVEL.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\MAIN.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\MAP.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\MAPWHO.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\MEMCARD.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\MOVE.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\MUSIC.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\OBJECT.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\OBJECTS.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\OPTMENU.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\PACKET.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\PERSON.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\POWERUP.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\PSXHOST.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\PSXIO.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\SCANNER.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\SCREENS.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\SEARCH.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\SHOT.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\SOUND.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\SSPRITE.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\STATS.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\SUPER.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\SWITCH.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\TEXT.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\THING.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\TRACK.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\VEHCOLID.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\VEHICLE.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\WEAPON.C.json")]
    [DataRow(@"C:\Files\GitHub\! PSX\DUMPSYM\TestData\WEATHER.C.json")]
    public void TopologicalSort(string path)
    {
        // BUG Level tpdef is wrong

        var list = UnitTestSplit222.GetSymbols(path);

        var split = Symbol.Split(list.ToArray());

        split = SymbolCleaner.PreProcessSymbols(split);
            
        var lists = split.Select(s => new Code(s.Select(t => (ISymbol)t.Record).ToList())).ToList();

        var symbols = lists.Select(s => new Sym { Code = s }).ToList();

        // AFFECT.C
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
            new Sym { Code = new Code(new SymbolRecordDef { Class = SymbolStorageClass.TPDEF, Type = new SymbolType { Value = 14 }, Name = "UINT" }) },
            new Sym { Code = new Code(new SymbolRecordDef { Class = SymbolStorageClass.TPDEF, Type = new SymbolType { Value = 15 }, Name = "ULONG" }) }
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
            sym.ResolveDependencies(symbols);
        }

        //Console.WriteLine(symbols.RemoveAll(s => s.Name == new SymKey(SymbolStorageClass.STRTAG, "SpriteData")));

        var sort = TopologicalSortWithPriority(symbols, out var result);

        foreach (var sym in result!)
        {
            Console.WriteLine(sym);
        }


        if (false)
        {
            List<Sym>? result2 = null;

            if (sort)
            {
                sort = TopologicalSortWithPriority(result, out result2);
            }

            if (result2 != null)
            {
                var sequenceEqual = result.SequenceEqual(result2);
                Assert.IsTrue(sequenceEqual);
            }
        }

        Assert.IsTrue(sort, "Topological sort failed.");
    }

    private static bool TopologicalSort(List<Sym> symbols, [MaybeNullWhen(false)] out List<Sym> result)
    {
        result = null;

        Dictionary<SymKey, Sym> symbolMap;

        Dictionary<SymKey, int> inDegree;

        var filter = true;

        if (filter) // BUG defined multiple times in STATS.C/THING.C
        {
            symbolMap = new Dictionary<SymKey, Sym>();

            inDegree = new Dictionary<SymKey, int>();

            foreach (var symbol in symbols)
            {
                var a = symbolMap.TryAdd(symbol.Name, symbol);

                var b = inDegree.TryAdd(symbol.Name, 0);

                if (a && b)
                {
                    continue;
                }

                Console.WriteLine($"Symbol is already in dictionary: {symbol}");
            }
        }
        else
        {
            symbolMap = symbols.ToDictionary(s => s.Name, s => s);

            inDegree = symbols.ToDictionary(s => s.Name, _ => 0);
        }

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
        if (sorted.Count != symbolMap.Count)
        {
            var list = symbolMap.Values.Where(x => !sorted.Contains(x)).ToList();
            result = list;

            return false;
        }

        result = sorted;

        return true;
    }

    private static bool TopologicalSortWithPriority(List<Sym> symbols, [MaybeNullWhen(false)] out List<Sym> result)
    {
        result = null;

        Dictionary<SymKey, Sym> symbolMap;

        Dictionary<SymKey, int> inDegree;

        var filter = true;

        if (filter) // BUG defined multiple times in STATS.C/THING.C
        {
            symbolMap = new Dictionary<SymKey, Sym>();

            inDegree = new Dictionary<SymKey, int>();

            foreach (var symbol in symbols)
            {
                var a = symbolMap.TryAdd(symbol.Name, symbol);

                var b = inDegree.TryAdd(symbol.Name, 0);

                if (a && b)
                {
                    continue;
                }

                Console.WriteLine($"Symbol is already in dictionary: {symbol}");
            }
        }
        else
        {
            symbolMap = symbols.ToDictionary(s => s.Name, s => s);

            inDegree = symbols.ToDictionary(s => s.Name, _ => 0);
        }

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

        //var queue = new Queue<SymKey>(inDegree.Where(s => s.Value == 0).Select(kv => kv.Key));

        var queue = new PriorityQueue<SymKey,int>();

        foreach (var pair in inDegree)
        {
            if (pair.Value == 0)
            {
                queue.Enqueue(pair.Key, symbolMap[pair.Key].Priority);
            }
        }

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
                    queue.Enqueue(neighbor, symbolMap[neighbor].Priority);
                }
            }
        }

        Console.WriteLine($"{nameof(sorted)}: {sorted.Count}");

        // Check for cycles
        if (sorted.Count != symbolMap.Count)
        {
            var list = symbolMap.Values.Where(x => !sorted.Contains(x)).ToList();
            result = list;

            return false;
        }

        result = sorted;

        return true;
    }
}