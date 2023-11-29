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
                case SymbolRecordBlockEnd symbolRecordBlockEnd:
                    continue; // TODO 1 node
                    throw new NotImplementedException(record.ToString());
                case SymbolRecordBlockStart symbolRecordBlockStart:
                    continue; // TODO block end

                    throw new NotImplementedException(record.ToString());
                case SymbolRecordDef symbolRecordDef:
                    node = ParseDef(node, symbolCollection);
                    continue;
                case SymbolRecordDef2 symbolRecordDef2:
                    node = ParseDef2(node, symbolCollection);
                    continue;
                case SymbolRecordEndSldInfo symbolRecordEndSldInfo:
                    continue; // TODO 1 node
                case SymbolRecordFunction2Start symbolRecordFunction2Start:
                    throw new NotImplementedException(record.ToString());
                case SymbolRecordFunctionEnd symbolRecordFunctionEnd:
                    continue; // TODO 1 node
                    throw new NotImplementedException(record.ToString());
                case SymbolRecordFunctionStart symbolRecordFunctionStart:
                    node = NewMethod(node);
                    continue; // TODO block start/end, function end + Def2 class AUTO type STRUCT*
                    throw new NotImplementedException(record.ToString());
                case SymbolRecordIncSldLineNum symbolRecordIncSldLineNum:
                    continue; // TODO 1 node
                case SymbolRecordIncSldLineNumByByte symbolRecordIncSldLineNumByByte:
                    continue; // TODO 1 node
                case SymbolRecordIncSldLineNumByWord symbolRecordIncSldLineNumByWord:
                    continue; // TODO 1 node
                    throw new NotImplementedException(record.ToString());
                case SymbolRecordName symbolRecordName:
                    continue; // TODO 1 node
                    throw new NotImplementedException(record.ToString());
                case SymbolRecordOverlay symbolRecordOverlay:
                    throw new NotImplementedException(record.ToString());
                case SymbolRecordSetOverlay symbolRecordSetOverlay:
                    throw new NotImplementedException(record.ToString());
                case SymbolRecordSetSldLineNum symbolRecordSetSldLineNum:
                    continue; // TODO 1 node
                case SymbolRecordSetSldToLineOfFile symbolRecordSetSldToLineOfFile:
                    continue; // TODO 1 node
                default:
                    throw new ArgumentOutOfRangeException(nameof(record));
            }
        }
    }

    private static LinkedListNode<Symbol> NewMethod(LinkedListNode<Symbol> node)
    {
        for (var current = node; current != null; current = current.Next)
        {
            var cvr = current.Value.Record;
            if (cvr is SymbolRecordFunctionStart)
            {
                continue;
            }

            if (cvr is SymbolRecordBlockStart)
            {
                continue;
            }

            if (cvr is SymbolRecordBlockEnd)
            {
                continue;
            }

            if (cvr is SymbolRecordDef)
            {
                continue;
            }

            if (cvr is SymbolRecordDef2)
            {
                continue;
            }

            if (cvr is SymbolRecordFunctionEnd)
            {
                return current;
            }

            throw new NotImplementedException(cvr.GetType() + " " + cvr);
        }

        throw new NotImplementedException(node.Value.Record.GetType().ToString());
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