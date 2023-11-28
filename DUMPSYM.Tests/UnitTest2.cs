using System.Diagnostics.CodeAnalysis;

namespace DUMPSYM.Tests;

[TestClass]
public sealed class UnitTest2 : UnitTestBase
{
    private static SymbolFile SymbolFile { get; set; } = null!;

    [ClassInitialize]
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
    public static void ClassInitialize(TestContext context)
    {
        using var stream = File.OpenRead(@"C:\Temp\PSX\SYM\FILE_023.SYM");

        SymbolFile = SymbolFile.Dump(stream);
    }

    [TestMethod]
    public void ProcessSymbols()
    {
        var symbols = new LinkedList<Symbol>(SymbolFile.Symbols);

        WriteLine(() => symbols, s => s.Count);

        for (var node = symbols.First; node != null; node = node.Next)
        {
            var record = node.Value.Record;

            switch (record)
            {
                case SymbolRecordBlockEnd:
                    break;
                case SymbolRecordBlockStart:
                    break;
                case SymbolRecordDef def:
                    switch (def.Class)
                    {
                        case SymbolStorageClass.TPDEF:
                            node = ParseTypeDefinition(node);
                            continue;
                        case SymbolStorageClass.STRTAG:
                            node = ParseStruct(node);
                            continue;
                        case SymbolStorageClass.UNTAG:
                            node = ParseUnion(node);
                            continue;
                        case SymbolStorageClass.EXT:
                            continue; // TODO only 1 node
                        case SymbolStorageClass.STAT:
                            continue; // TODO only 1 node
                    }

                    break;
                case SymbolRecordDef2 def2:
                    if (def2.Class is SymbolStorageClass.TPDEF)
                        continue; // TODO only 1 node
                    if (def2.Class is SymbolStorageClass.EXT)
                        continue; // TODO only 1 node
                    if (def2.Class is SymbolStorageClass.STAT)
                        continue; // TODO only 1 node
                    break;
                case SymbolRecordEndSldInfo:
                    continue;
                case SymbolRecordFunction2Start:
                    break;
                case SymbolRecordFunctionEnd:
                    break;
                case SymbolRecordFunctionStart:
                    node = ParseFunction(node);
                    continue;
                case SymbolRecordIncSldLineNum:
                    continue;
                case SymbolRecordIncSldLineNumByByte:
                    continue;
                case SymbolRecordIncSldLineNumByWord:
                    continue;
                case SymbolRecordName:
                    continue; // TODO only 1 node
                case SymbolRecordOverlay:
                    break;
                case SymbolRecordSetOverlay:
                    break;
                case SymbolRecordSetSldLineNum:
                    continue;
                case SymbolRecordSetSldToLineOfFile:
                    continue;
                default:
                    throw new ArgumentOutOfRangeException(nameof(record));
            }

            throw new NotImplementedException(node.Value.ToString());
        }
    }

    private LinkedListNode<Symbol> ParseFunction(LinkedListNode<Symbol> symbolNode)
    {
        var symbol = symbolNode.Value;

        if (symbol.Record is not SymbolRecordFunctionStart def)
        {
            throw new ArgumentOutOfRangeException(nameof(symbolNode));
        }

        WriteLine($"Parsing function {def.Name}");

        for (var node = symbolNode; node != null; node = node.Next)
        {
            var record = node.Value.Record;

            if (record is SymbolRecordFunctionEnd)
            {
                return node;
            }
        }

        throw new InvalidDataException();
    }

    private LinkedListNode<Symbol> ParseUnion(LinkedListNode<Symbol> symbolNode)
    {
        var symbol = symbolNode.Value;

        if (symbol.Record is not SymbolRecordDef { Class: SymbolStorageClass.UNTAG } def)
        {
            throw new ArgumentOutOfRangeException(nameof(symbolNode));
        }

        WriteLine($"Parsing union {def.Name}");

        for (var node = symbolNode; node != null; node = node.Next)
        {
            var record = node.Value.Record;

            if (record is SymbolRecordDef2 { Class: SymbolStorageClass.EOS, Name: ".eos" } end && end.Tag == def.Name)
            {
                return node;
            }
        }

        throw new InvalidOperationException();
    }

    private LinkedListNode<Symbol> ParseStruct(LinkedListNode<Symbol> symbolNode)
    {
        var symbol = symbolNode.Value;

        if (symbol.Record is not SymbolRecordDef { Class: SymbolStorageClass.STRTAG } def)
        {
            throw new ArgumentOutOfRangeException(nameof(symbolNode));
        }

        WriteLine($"Parsing struct {def.Name}");

        for (var node = symbolNode; node != null; node = node.Next)
        {
            var record = node.Value.Record;

            if (record is SymbolRecordDef2 { Class: SymbolStorageClass.EOS, Name: ".eos" } end && end.Tag == def.Name)
            {
                return node;
            }
        }

        throw new InvalidOperationException();
    }

    private LinkedListNode<Symbol> ParseTypeDefinition(LinkedListNode<Symbol> symbolNode)
    {
        var symbol = symbolNode.Value;

        if (symbol.Record is not SymbolRecordDef { Class: SymbolStorageClass.TPDEF })
        {
            throw new ArgumentOutOfRangeException(nameof(symbolNode));
        }

        // TODO

        return symbolNode;
    }
}