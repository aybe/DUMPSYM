using System.Reflection;

namespace DUMPSYM.Tests;

[TestClass]
public sealed class UnitTest3 : UnitTestBase
{
    public static IEnumerable<object[]> DumpSymFileData()
    {
        var path = UnitTestDataUtility.GetFullPath(".SYM file list.txt");

        var text = File.ReadAllLines(path);

        foreach (var line in text)
        {
            if (File.Exists(line))
            {
                yield return new object[] { line };
            }
        }
    }

    public static string DumpSymFileName(MethodInfo methodInfo, object[] data)
    {
        return $"{methodInfo.Name} {Path.GetFileName(data[0].ToString()!)}";
    }

    [TestMethod]
    [DynamicData(nameof(DumpSymFileData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(DumpSymFileName))]
    public void DumpSymFile(string path)
    {
        using var stream = File.OpenRead(path);

        var file = SymbolFile.Dump(stream);

        var symbolCollection = new SymbolCollection();

        var symbols = new LinkedList<Symbol>(file.Symbols);

        for (var node = symbols.First; node != null; node = node.Next)
        {
            var record = node.Value.Record;

            switch (record)
            {
                case SymbolRecordBlockEnd:
                    continue; // TODO 1 node
                case SymbolRecordBlockStart:
                    continue; // TODO block end
                case SymbolRecordDef:
                    node = ParseDef(node, symbolCollection);
                    continue;
                case SymbolRecordDef2:
                    node = ParseDef2(node, symbolCollection);
                    continue;
                case SymbolRecordEndSldInfo:
                    continue; // TODO 1 node
                case SymbolRecordFunction2Start:
                    node = ProcessFunction2Start(node);
                    continue;
                case SymbolRecordFunctionEnd:
                    continue; // TODO 1 node
                case SymbolRecordFunctionStart:
                    node = ProcessFunctionStart(node);
                    continue; // TODO block start/end, function end + Def2 class AUTO type STRUCT*
                case SymbolRecordIncSldLineNum:
                    continue; // TODO 1 node
                case SymbolRecordIncSldLineNumByByte:
                    continue; // TODO 1 node
                case SymbolRecordIncSldLineNumByWord:
                    continue; // TODO 1 node
                case SymbolRecordName:
                    continue; // TODO 1 node
                case SymbolRecordOverlay:
                    continue; // TODO 1 node
                case SymbolRecordSetOverlay:
                    continue; // TODO 1 node
                case SymbolRecordSetSldLineNum:
                    continue; // TODO 1 node
                case SymbolRecordSetSldToLineOfFile:
                    continue; // TODO 1 node
                default:
                    throw new NotImplementedException(node.Value.ToString());
            }
        }
    }

    private static LinkedListNode<Symbol> ProcessFunctionStart(LinkedListNode<Symbol> node) // TODO merge with other?
    {
        for (var current = node; current != null; current = current.Next)
        {
            var record = current.Value.Record;

            switch (record)
            {
                case SymbolRecordFunctionStart:
                case SymbolRecordBlockStart:
                case SymbolRecordBlockEnd:
                case SymbolRecordDef:
                case SymbolRecordDef2:
                    continue;
                case SymbolRecordFunctionEnd:
                    return current;
                default:
                    throw new NotImplementedException(node.Value.ToString());
            }
        }

        throw new NotImplementedException(node.Value.ToString());
    }

    private static LinkedListNode<Symbol> ProcessFunction2Start(LinkedListNode<Symbol> node) // TODO merge with other?
    {
        for (var current = node; current != null; current = current.Next)
        {
            var record = current.Value.Record;

            switch (record)
            {
                case SymbolRecordFunction2Start:
                case SymbolRecordBlockStart:
                case SymbolRecordBlockEnd:
                case SymbolRecordDef:
                case SymbolRecordDef2:
                    continue;
                case SymbolRecordFunctionEnd:
                    return current;
                default:
                    throw new NotImplementedException(node.Value.ToString());
            }
        }

        throw new NotImplementedException(node.Value.ToString());
    }

    private LinkedListNode<Symbol> ParseDef2(LinkedListNode<Symbol> node, SymbolCollection symbolCollection)
    {
        if (node.Value.Record is SymbolRecordDef2 def2)
        {
            if (def2.Class is SymbolStorageClass.TPDEF)
            {
                symbolCollection.TypeDefinitions.Add(new LinkedList<Symbol>(new[] { node.Value }));
                return node;
            }

            if (def2.Class is SymbolStorageClass.EXT)
            {
                symbolCollection.Externals.Add(new LinkedList<Symbol>(new[] { node.Value }));
                return node;
            }

            if (def2.Class is SymbolStorageClass.STAT)
            {
                symbolCollection.Statics.Add(new LinkedList<Symbol>(new[] { node.Value }));
                return node;
            }
        }

        throw new InvalidOperationException(node.Value.ToString());
    }

    private LinkedListNode<Symbol> ParseDef(
        LinkedListNode<Symbol> node, SymbolCollection collection)
    {
        var symbol = node.Value;
        if (symbol.Record is SymbolRecordDef def)
        {
            if (def.Class is SymbolStorageClass.TPDEF)
            {
                collection.TypeDefinitions.Add(new LinkedList<Symbol>(new[] { symbol }));
                return node;
            }

            if (def.Class is SymbolStorageClass.ENTAG)
            {
                var list = new LinkedList<Symbol>();

                for (var current = node.Next; current != null; current = current.Next)
                {
                    switch (current.Value.Record)
                    {
                        case SymbolRecordDef { Class: SymbolStorageClass.ENTAG, Type.Kind: SymbolTypeKind.ENUM }:
                            list.AddLast(current.Value);
                            continue;
                        case SymbolRecordDef { Class: SymbolStorageClass.MOE }:
                            list.AddLast(current.Value);
                            continue;
                        case SymbolRecordDef2 { Class: SymbolStorageClass.EOS , Type.Kind: SymbolTypeKind.NULL}:
                            list.AddLast(current.Value);
                            collection.Structures.Add(list);
                            return current;
                    }
                }
            }

            if (def.Class is SymbolStorageClass.STRTAG)
            {
                var list = new LinkedList<Symbol>();

                for (var current = node.Next; current != null; current = current.Next)
                {
                    switch (current.Value.Record)
                    {
                        case SymbolRecordDef { Class: SymbolStorageClass.STRTAG, Type.Kind: SymbolTypeKind.STRUCT }:
                            list.AddLast(current.Value);
                            continue;
                        case SymbolRecordDef { Class: SymbolStorageClass.MOS }:
                            list.AddLast(current.Value);
                            continue;
                        case SymbolRecordDef2 { Class: SymbolStorageClass.EOS }:
                            list.AddLast(current.Value);
                            collection.Structures.Add(list);
                            return current;
                    }
                }
            }

            if (def.Class is SymbolStorageClass.UNTAG)
            {
                var list = new LinkedList<Symbol>();

                for (var current = node.Next; current != null; current = current.Next)
                {
                    switch (current.Value.Record)
                    {
                        case SymbolRecordDef { Class: SymbolStorageClass.UNTAG, Type.Kind: SymbolTypeKind.UNION }:
                            list.AddLast(current.Value);
                            continue;
                        case SymbolRecordDef { Class: SymbolStorageClass.MOU }:
                            list.AddLast(current.Value);
                            continue;
                        case SymbolRecordDef2 { Class: SymbolStorageClass.EOS }:
                            list.AddLast(current.Value);
                            collection.Unions.Add(list);
                            return current;
                    }
                }
            }

            if (def.Class is SymbolStorageClass.EXT)
            {
                collection.Externals.Add(new LinkedList<Symbol>(new[] { symbol }));
                return node;
            }

            if (def.Class is SymbolStorageClass.STAT)
            {
                collection.Statics.Add(new LinkedList<Symbol>(new[] { symbol }));
                return node;
            }

            if (def.Class is SymbolStorageClass.FILE)
            {
                return node;
            }
        }

        throw new NotImplementedException(node.Value.Record.ToString());
    }
}

public class SymbolCollection
{
    public List<LinkedList<Symbol>> Externals { get; set; } = new(); // TODO only 1 node

    public List<LinkedList<Symbol>> Statics { get; set; } = new(); // TODO only 1 node

    public List<LinkedList<Symbol>> TypeDefinitions { get; set; } = new(); // TODO only 1 node

    public List<LinkedList<Symbol>> Structures { get; set; } = new();

    public List<LinkedList<Symbol>> Unions { get; set; } = new();
}