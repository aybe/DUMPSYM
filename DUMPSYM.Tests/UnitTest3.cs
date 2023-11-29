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

        var symbols = new LinkedList<Symbol>(file.Symbols);

        var registry = new SymbolRegistry();

        for (var node = symbols.First; node != null; node = node.Next)
        {
            var record = node.Value.Record;

            switch (record)
            {
                case ISymbolDefinition:
                    node = ParseDefinition(node, registry);
                    continue;
                case SymbolRecordEndSldInfo:
                    continue; // TODO 1 node
                case ISymbolFunction:
                    node = ParseFunction(node, registry.Functions);
                    continue;
                case ISymbolLineModifier:
                    continue; // TODO 1 node
                case ISymbolName:
                    continue; // TODO 1 node
                case ISymbolOverlay:
                    continue; // TODO 1 node
                default:
                    throw new NotImplementedException(node.Value.ToString());
            }
        }
    }

    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "performance")]
    private static LinkedListNode<Symbol> ParseFunction(LinkedListNode<Symbol> source, List<LinkedList<Symbol>> target)
    {
        var list = new LinkedList<Symbol>();

        for (var node = source; node != null; node = node.Next)
        {
            var symbol = node.Value;

            switch (symbol.Record)
            {
                case ISymbolFunction:
                    list.AddLast(symbol);
                    continue;
                case ISymbolFunctionBlock:
                    list.AddLast(symbol);
                    continue;
                case ISymbolDefinition:
                    list.AddLast(symbol); // TODO should use ParseDefinition
                    continue;
                case ISymbolFunctionEnd:
                    list.AddLast(symbol);
                    target.Add(list);
                    return node;
                default:
                    throw new NotImplementedException(symbol.ToString());
            }
        }

        throw new NotImplementedException(source.Value.ToString());
    }

    [SuppressMessage("ReSharper", "ConvertSwitchStatementToSwitchExpression")]
    [SuppressMessage("ReSharper", "SwitchStatementHandlesSomeKnownEnumValuesWithDefault")]
    [SuppressMessage("Style", "IDE0066:Convert switch statement to expression", Justification = "code coverage")]
    private static LinkedListNode<Symbol> ParseDefinition(LinkedListNode<Symbol> node, SymbolRegistry registry)
    {
        var symbol = node.Value;

        if (symbol.Record is not ISymbolDefinition def)
        {
            throw new ArgumentOutOfRangeException(nameof(node));
        }

        switch (def.Class)
        {
            case SymbolStorageClass.ENTAG:
                return ParseType(node, registry.Enumerations);
            case SymbolStorageClass.STRTAG:
                return ParseType(node, registry.Structures);
            case SymbolStorageClass.UNTAG:
                return ParseType(node, registry.Unions);
            case SymbolStorageClass.EXT:
                return ParseItem(node, registry.Externals);
            case SymbolStorageClass.FILE:
                return ParseItem(node, registry.Files);
            case SymbolStorageClass.REG:
                return ParseItem(node, registry.Registers);
            case SymbolStorageClass.STAT:
                return ParseItem(node, registry.Statics);
            case SymbolStorageClass.TPDEF:
                return ParseItem(node, registry.TypeDefinitions);
            default:
                throw new NotImplementedException(symbol.ToString());
        }
    }

    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "performance")]
    private static LinkedListNode<Symbol> ParseItem(LinkedListNode<Symbol> node, List<Symbol> list)
    {
        var symbol = node.Value;

        list.Add(symbol);

        return node;
    }

    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "performance")]
    private static LinkedListNode<Symbol> ParseType(LinkedListNode<Symbol> source, List<LinkedList<Symbol>> target)
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

        throw new NotImplementedException(source.Value.ToString());
    }
}