using System.Diagnostics.CodeAnalysis;
using System.Text;
using DUMPSYM.Output;

namespace DUMPSYM.Tests;

[TestClass]
public sealed class UnitTest2 : UnitTestBase
{
    private static SymbolFile SymbolFile { get; set; } = null!;

    private static bool PrintStructures => true;

    private static bool PrintUnions => true;

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

        var structures = new List<MemberCollection>();

        var unions = new List<MemberCollection>();

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
                            node = ParseStruct(node, structures);
                            continue;
                        case SymbolStorageClass.UNTAG:
                            node = ParseUnion(node, unions);
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

        if (PrintStructures)
        {
            WriteLine(() => structures, s => s.Count);

            foreach (var collection in structures)
            {
                WriteLine(collection.Print());
            }
        }

        if (PrintUnions)
        {
            WriteLine(() => unions, s => s.Count);

            foreach (var collection in unions)
            {
                WriteLine(collection.Print());
            }
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

    private LinkedListNode<Symbol> ParseUnion(LinkedListNode<Symbol> symbolNode, List<MemberCollection> list)
    {
        var symbol = symbolNode.Value;

        Assert.IsTrue(symbol.Record is SymbolRecordDef { Class: SymbolStorageClass.UNTAG });

        MemberCollection? collection = null;

        for (var node = symbolNode; node != null; node = node.Next)
        {
            var record = node.Value.Record;

            if (record is not ISymbolDefinition definition)
            {
                throw new InvalidDataException();
            }

            switch (definition.Class)
            {
                case SymbolStorageClass.UNTAG:
                    Assert.IsNull(collection);
                    collection = new MemberCollection("union", definition.Name);
                    break;
                case SymbolStorageClass.MOU:
                    Assert.IsNotNull(collection);
                    ParseMember(collection, definition);
                    break;
                case SymbolStorageClass.EOS:
                    Assert.IsNotNull(collection);
                    list.Add(collection);
                    return node;
                default:
                    throw new InvalidDataException();
            }
        }

        throw new InvalidOperationException();
    }

    private LinkedListNode<Symbol> ParseStruct(LinkedListNode<Symbol> symbolNode, List<MemberCollection> list)
        // TODO add comments to struct members?
    {
        var symbol = symbolNode.Value;

        if (symbol.Record is not SymbolRecordDef { Class: SymbolStorageClass.STRTAG } def)
        {
            throw new ArgumentOutOfRangeException(nameof(symbolNode));
        }

        MemberCollection? collection = null;

        for (var node = symbolNode; node != null; node = node.Next)
        {
            var record = node.Value.Record;

            if (record is not ISymbolDefinition definition)
            {
                throw new InvalidDataException();
            }

            if (definition is { Class: SymbolStorageClass.EOS, Name: ".eos" })
            {
                Assert.AreEqual(definition.Tag, def.Name);
                Assert.IsNotNull(collection);
                list.Add(collection);
                return node;
            }

            if (definition.Class == SymbolStorageClass.STRTAG)
            {
                Assert.IsNull(collection);
                collection = new MemberCollection("struct", definition.Name);
            }
            else
            {
                Assert.IsNotNull(collection);
                ParseMember(collection, definition);
            }
        }

        throw new InvalidOperationException();
    }

    private void ParseMember(MemberCollection collection, ISymbolDefinition definition)
    {
        var builder = new StringBuilder();

        builder.Append(definition.Type.Kind switch
        {
            SymbolTypeKind.NULL   => "null",
            SymbolTypeKind.VOID   => "void",
            SymbolTypeKind.CHAR   => "char",
            SymbolTypeKind.SHORT  => "short",
            SymbolTypeKind.INT    => "int",
            SymbolTypeKind.LONG   => "long",
            SymbolTypeKind.FLOAT  => "float",
            SymbolTypeKind.DOUBLE => "double",
            SymbolTypeKind.STRUCT => "struct",
            SymbolTypeKind.UNION  => "union",
            SymbolTypeKind.ENUM   => "enum",
            SymbolTypeKind.MOE    => "enum member",
            SymbolTypeKind.UCHAR  => "unsigned char",
            SymbolTypeKind.USHORT => "unsigned short",
            SymbolTypeKind.UINT   => "unsigned int",
            SymbolTypeKind.ULONG  => "unsigned long",
            _                     => throw new NotSupportedException(definition.Type.Kind.ToString())
        });

        builder.Append(' ');

        if (definition.Tag != string.Empty)
        {
            builder.Append($"{definition.Tag} ");
        }

        var modifiers = definition.Type.Modifiers.ToArray();

        var functionPointer = modifiers.Contains(SymbolTypeModifier.FCN);

        if (functionPointer)
        {
            Assert.AreEqual(1, modifiers.Count(s => s is SymbolTypeModifier.FCN), "Multiple function pointers not implemented.");
        }

        if (functionPointer)
        {
            builder.Append('(');
        }

        foreach (var modifier in modifiers)
        {
            if (modifier == SymbolTypeModifier.PTR)
            {
                builder.Append('*');
            }
        }

        builder.Append(definition.Name);

        if (functionPointer)
        {
            builder.Append(")()");
        }

        switch (definition.Class)
        {
            case SymbolStorageClass.MOS:
            case SymbolStorageClass.MOU:
                foreach (var dimension in definition.Dimensions.Reverse())
                {
                    builder.Append($"[{dimension}]");
                }

                break;
            case SymbolStorageClass.FIELD:
                builder.Append($" : {definition.Size}");
                break;
            default:
                throw new NotImplementedException(definition.Class.ToString());
        }

        builder.Append(';');

        collection.Members.Add(new Member(definition.Name) { Text = builder.ToString() });
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