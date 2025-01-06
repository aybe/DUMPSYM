using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace DUMPSYM.Tests;

[TestClass]
public sealed class UnitTestGenerate : UnitTestBase
{
    private static bool PrintTypedefs => false;

    [TestMethod]
    public void TestTypedefs()
    {
        var list = new LinkedList<SymbolRecord>(UnitTest1.GetSample());

        PrintCount(list);

        var typedefs = list.OfType<SymbolRecordDef>().Where(s => s.Class is SymbolStorageClass.TPDEF).ToArray();

        Console.WriteLine($"Typedefs (raw): {typedefs.Length}");

        list = new LinkedList<SymbolRecord>(list.Except(typedefs));

        typedefs = typedefs.Distinct(SymbolRecordDefEqualityComparer.Instance).ToArray();

        Console.WriteLine($"Typedefs (distinct): {typedefs.Length}");

        PrintCount(list);

        if (PrintTypedefs)
        {
            foreach (var def in typedefs)
            {
                Console.WriteLine(GetTypedefString(def));
            }
        }

        // TODO Def2 after structs

        var result = new List<LinkedListNode<SymbolRecord>>();

        var node = list.First;

        while (node != null)
        {
            if (!TryFindStruct(node, result))
            {
                break;
            }

            node = result.Last().Next;

            result.ForEach(list.Remove);

            result.Clear();
        }

        PrintCount(list);

        foreach (var record in list)
        {
            Console.WriteLine(record);
        }
    }

    private static void PrintCount<T>(ICollection<T> collection)
    {
        Console.WriteLine($"Count: {collection.Count}");
    }

    private static string GetTypedefString(SymbolRecordDef def)
    {
        Assert.AreEqual(0u, def.Size);

        Assert.AreEqual(SymbolStorageClass.TPDEF, def.Class);

        var handler = new DefaultInterpolatedStringHandler();

        handler.AppendLiteral("typedef ");

        handler.AppendLiteral(GetKindString(def.Type.Kind));

        var modifiers = def.Type.Modifiers.ToArray();

        if (modifiers.Any(s => s == SymbolTypeModifier.ARY))
        {
            throw new NotImplementedException(SymbolTypeModifier.ARY.ToString());
        }

        var fcn = modifiers.Any(s => s == SymbolTypeModifier.FCN);

        if (fcn)
        {
            handler.AppendLiteral(" (");
        }

        if (modifiers.Any(s => s == SymbolTypeModifier.PTR))
        {
            handler.AppendLiteral("*");
        }

        if (!fcn)
        {
            handler.AppendLiteral(" ");
        }

        handler.AppendFormatted(def.Name);

        if (fcn)
        {
            handler.AppendLiteral(")(void)");
        }

        handler.AppendLiteral(";");

        var str = handler.ToStringAndClear();

        return str;
    }

    private static string GetKindString(SymbolTypeKind kind)
    {
        return kind switch
        {
            SymbolTypeKind.NULL   => null,
            SymbolTypeKind.VOID   => "void",
            SymbolTypeKind.CHAR   => "char",
            SymbolTypeKind.SHORT  => "short",
            SymbolTypeKind.INT    => "int",
            SymbolTypeKind.LONG   => "long",
            SymbolTypeKind.FLOAT  => "float",
            SymbolTypeKind.DOUBLE => "double",
            SymbolTypeKind.STRUCT => null,
            SymbolTypeKind.UNION  => null,
            SymbolTypeKind.ENUM   => null,
            SymbolTypeKind.MOE    => null,
            SymbolTypeKind.UCHAR  => "unsigned char",
            SymbolTypeKind.USHORT => "unsigned short",
            SymbolTypeKind.UINT   => "unsigned int",
            SymbolTypeKind.ULONG  => "unsigned long",
            _                     => null
        } ?? throw new NotSupportedException(kind.ToString());
    }

    private static bool TryFindNode<T>(
        LinkedListNode<T> node, [MaybeNullWhen(false)] out LinkedListNode<T> result, Func<T, bool> predicate)
    {
        result = default;

        var current = node;

        while (current != null)
        {
            if (predicate(current.Value))
            {
                result = current;

                return true;
            }

            current = current.Next;
        }

        return false;
    }

    private static bool TryFindStruct(
        LinkedListNode<SymbolRecord> node, List<LinkedListNode<SymbolRecord>> list)
    {
        if (!TryFindNode(node, out var head, s => s is ISymbolDefinition { Class: SymbolStorageClass.STRTAG }))
        {
            return false;
        }

        if (!TryFindNode(head, out var tail, s => s is ISymbolDefinition { Class: SymbolStorageClass.EOS }))
        {
            return false;
        }

        list.Clear();

        var next = head;

        while (next != null && next != tail)
        {
            list.Add(next);

            next = next.Next;
        }

        list.Add(tail);

        return true;
    }
}