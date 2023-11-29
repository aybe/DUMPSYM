using System.Diagnostics.CodeAnalysis;
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
                case ISymbolDefinition:
                    node = ParseDefinition(node, symbolCollection);
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

        CheckCollection(symbolCollection);
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

    [SuppressMessage("ReSharper", "ConvertSwitchStatementToSwitchExpression")]
    [SuppressMessage("ReSharper", "SwitchStatementHandlesSomeKnownEnumValuesWithDefault")]
    [SuppressMessage("Style", "IDE0066:Convert switch statement to expression", Justification = "code coverage")]
    private static LinkedListNode<Symbol> ParseDefinition(LinkedListNode<Symbol> node, SymbolCollection collection)
    {
        var symbol = node.Value;

        if (symbol.Record is not ISymbolDefinition def)
        {
            throw new ArgumentOutOfRangeException(nameof(node));
        }

        switch (def.Class)
        {
            case SymbolStorageClass.ENTAG:
                return ParseType(node, collection.Enumerations);
            case SymbolStorageClass.STRTAG:
                return ParseType(node, collection.Structures);
            case SymbolStorageClass.UNTAG:
                return ParseType(node, collection.Unions);
            case SymbolStorageClass.EXT:
                return ParseItem(node, collection.Externals);
            case SymbolStorageClass.FILE:
                return ParseItem(node, collection.Files);
            case SymbolStorageClass.REG:
                return ParseItem(node, collection.Registers);
            case SymbolStorageClass.STAT:
                return ParseItem(node, collection.Statics);
            case SymbolStorageClass.TPDEF:
                return ParseItem(node, collection.TypeDefinitions);
            default:
                throw new NotImplementedException(node.Value.Record.ToString());
        }
    }

    private static LinkedListNode<Symbol> ParseItem(LinkedListNode<Symbol> node, ICollection<Symbol> list)
    {
        var symbol = node.Value;

        list.Add(symbol);

        return node;
    }

    private static LinkedListNode<Symbol> ParseType(LinkedListNode<Symbol> source, ICollection<LinkedList<Symbol>> target)
    {
        var list = new LinkedList<Symbol>();

        for (var node = source; node != null; node = node.Next)
        {
            var symbol = node.Value;

            switch (symbol.Record)
            {
                case ISymbolDefinition { Class: SymbolStorageClass.ENTAG or SymbolStorageClass.STRTAG or SymbolStorageClass.UNTAG }:
                    list.AddLast(symbol);
                    continue;
                case ISymbolDefinition { Class: SymbolStorageClass.FIELD or SymbolStorageClass.MOE or SymbolStorageClass.MOS or SymbolStorageClass.MOU }:
                    list.AddLast(symbol);
                    continue;
                case ISymbolDefinition { Class: SymbolStorageClass.EOS }:
                    list.AddLast(symbol);
                    target.Add(list);
                    return node;
                default:
                    throw new NotImplementedException(symbol.ToString());
            }
        }

        throw new InvalidOperationException();
    }

    private static void CheckCollection(SymbolCollection collection)
    {
        foreach (var list in collection.Enumerations)
        {
            Assert.AreNotEqual(1, list.Count);
        }

        foreach (var list in collection.Structures)
        {
            Assert.AreNotEqual(1, list.Count);
        }

        foreach (var list in collection.Unions)
        {
            Assert.AreNotEqual(1, list.Count);
        }
    }
}

public sealed class SymbolCollection
{
    public IList<Symbol> Externals { get; } = new List<Symbol>();

    public IList<Symbol> Files { get; } = new List<Symbol>();

    public IList<Symbol> Registers { get; } = new List<Symbol>();

    public IList<Symbol> Statics { get; } = new List<Symbol>();

    public IList<Symbol> TypeDefinitions { get; } = new List<Symbol>();

    public IList<LinkedList<Symbol>> Enumerations { get; } = new List<LinkedList<Symbol>>();

    public IList<LinkedList<Symbol>> Structures { get; } = new List<LinkedList<Symbol>>();

    public IList<LinkedList<Symbol>> Unions { get; } = new List<LinkedList<Symbol>>();
}