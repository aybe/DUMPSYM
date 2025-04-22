using System.Diagnostics.CodeAnalysis;

// ReSharper disable StringLiteralTypo
// ReSharper disable ArrangeTrailingCommaInMultilineLists
// ReSharper disable CommentTypo

namespace DUMPSYM.Tests;

[TestClass]
public sealed partial class UnitTestSymbolSort : UnitTestBase
{
    private SortingSettings Settings { get; } = new()
    {
        SortByFilePosition = true
    };

    private void TopologicalSort(string path)
    {
        // BUG Level tpdef is wrong

        var list = UnitTestSplit222.GetSymbols(path);

        var split = Symbol.Split(list.ToArray());

        split = SymbolCleaner.PreProcessSymbols(split);

        var lists = split.Select((s, t) => new Code(s.Select(u => (ISymbol)u.Record).ToList(), t)).ToList();

        var symbols = lists.Select(s => new Sym { Code = s }).ToList();

        AddMissingSymbols(symbols);

        Sym.Initialize();

        foreach (var sym in symbols)
        {
            sym.Settings = Settings;

            sym.ResolveDependencies(symbols);

            //  if (settings.SortByFilePosition) // TODO delete
            // {
            //     sym.Name.Position = sym.Code.Position; // TODO this sucks
            // }
        }

        //Console.WriteLine(symbols.RemoveAll(s => s.Name == new SymKey(SymbolStorageClass.STRTAG, "SpriteData")));

        var sort = TopologicalSortWithPriority(symbols, out var result, Settings);

        foreach (var sym in result!)
        {
            Console.WriteLine(sym);
        }


        if (false)
        {
            List<Sym>? result2 = null;

            if (sort)
            {
                sort = TopologicalSortWithPriority(result, out result2, Settings);
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

    private static bool TopologicalSortWithPriority(List<Sym> symbols, [MaybeNullWhen(false)] out List<Sym> result, SortingSettings settings)
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

        var queue = new PriorityQueue<SymKey, int>();

        var pairs = inDegree
            .Where(s => s.Value == 0)
            .OrderBy(s => settings.SortByFilePosition ? symbolMap[s.Key].Code.Position : 0);

        foreach (var (key, _) in pairs)
        {
            queue.Enqueue(key, symbolMap[key].Priority);
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

    private static void AddMissingSymbols(List<Sym> symbols)
    {
        // AFFECT.C

        symbols.InsertRange(0,
        [
            new Sym { Code = new Code(new SymbolRecordDef(SymbolStorageClass.TPDEF, new SymbolType(SymbolTypeKind.NULL), 0, nameof(SymbolTypeKind.NULL))) },
            new Sym { Code = new Code(new SymbolRecordDef(SymbolStorageClass.TPDEF, new SymbolType(SymbolTypeKind.VOID), 0, nameof(SymbolTypeKind.VOID))) },
            new Sym { Code = new Code(new SymbolRecordDef(SymbolStorageClass.TPDEF, new SymbolType(SymbolTypeKind.CHAR), 0, nameof(SymbolTypeKind.CHAR))) },
            new Sym { Code = new Code(new SymbolRecordDef(SymbolStorageClass.TPDEF, new SymbolType(SymbolTypeKind.SHORT), 0, nameof(SymbolTypeKind.SHORT))) },
            new Sym { Code = new Code(new SymbolRecordDef(SymbolStorageClass.TPDEF, new SymbolType(SymbolTypeKind.INT), 0, nameof(SymbolTypeKind.INT))) },
            new Sym { Code = new Code(new SymbolRecordDef(SymbolStorageClass.TPDEF, new SymbolType(SymbolTypeKind.LONG), 0, nameof(SymbolTypeKind.LONG))) },
            new Sym { Code = new Code(new SymbolRecordDef(SymbolStorageClass.TPDEF, new SymbolType(SymbolTypeKind.FLOAT), 0, nameof(SymbolTypeKind.FLOAT))) },
            new Sym { Code = new Code(new SymbolRecordDef(SymbolStorageClass.TPDEF, new SymbolType(SymbolTypeKind.DOUBLE), 0, nameof(SymbolTypeKind.DOUBLE))) },
            new Sym { Code = new Code(new SymbolRecordDef(SymbolStorageClass.TPDEF, new SymbolType(SymbolTypeKind.UCHAR), 0, nameof(SymbolTypeKind.UCHAR))) },
            new Sym { Code = new Code(new SymbolRecordDef(SymbolStorageClass.TPDEF, new SymbolType(SymbolTypeKind.USHORT), 0, nameof(SymbolTypeKind.USHORT))) },
            new Sym { Code = new Code(new SymbolRecordDef(SymbolStorageClass.TPDEF, new SymbolType(SymbolTypeKind.UINT), 0, nameof(SymbolTypeKind.UINT))) },
            new Sym { Code = new Code(new SymbolRecordDef(SymbolStorageClass.TPDEF, new SymbolType(SymbolTypeKind.ULONG), 0, nameof(SymbolTypeKind.ULONG))) }
        ]);

        var ncb = new Code([ // TODO why is this not at the top of game symbols as it only depends on primitives?
            new SymbolRecordDef(SymbolStorageClass.STRTAG, new SymbolType(SymbolTypeKind.STRUCT), 4, "NCB"),
            new SymbolRecordDef(SymbolStorageClass.MOS, new SymbolType(SymbolTypeKind.INT), 4, "BAD_SYMBOL"),
            new SymbolRecordDef2(SymbolStorageClass.EOS, new SymbolType(SymbolTypeKind.NULL), 4, [], "NCB", ".eos")
        ]);

        symbols.Add(new Sym { Code = ncb });
    }
}