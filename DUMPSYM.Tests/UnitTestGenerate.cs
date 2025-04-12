using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo

namespace DUMPSYM.Tests;

[TestClass]
public sealed partial class UnitTestGenerate : UnitTestBase
{
    private static bool PrintRemaining => false;

    private static bool PrintTypedefs => true;

    private static bool RemoveLineModifiers => true;

    private static bool RemoveUnions => true;

    private static bool RemoveFunctions => true;

    private static bool RemoveNames => true;

    [GeneratedRegex(@"^\.\d+fake$")]
    private static partial Regex RegexFakeName();

    [TestMethod]
    public void TestTypedefs()
    {
        var list = new LinkedList<SymbolRecord>(UnitTest1.GetSample());

        {
            Console.WriteLine("Reading symbols...");
            PrintCount(list);
            Console.WriteLine();
        }

        if (RemoveLineModifiers)
        {
            Console.WriteLine("Removing line modifiers...");
            RemoveWhere(list, s => s is ISymbolLineModifier);
            PrintCount(list);
            Console.WriteLine();
        }

        if (RemoveUnions)
        {
            Console.WriteLine("Removing unions...");

            while (Remove(list, s => s is ISymbolDefinition { Class: SymbolStorageClass.UNTAG },
                       s => s is ISymbolDefinition { Class: SymbolStorageClass.EOS }) != null)
            {
            }

            PrintCount(list);
            Console.WriteLine();
        }

        if (RemoveFunctions)
        {
            Console.WriteLine("Removing functions...");

            while (Remove(list, s => s is SymbolRecordFunctionStart, s => s is SymbolRecordFunctionEnd) != null)
            {
            }

            PrintCount(list);
            Console.WriteLine();
        }

        if (RemoveNames)
        {
            Console.WriteLine("Removing names...");
            RemoveWhere(list, s => s is SymbolRecordName);
            PrintCount(list);
            Console.WriteLine();
        }

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

            Console.WriteLine(GetStructString(result));

            node = result.Last().Next;

            result.ForEach(list.Remove);

            result.Clear();
        }

        PrintCount(list);

        if (PrintRemaining)
        {
            foreach (var record in list)
            {
                Console.WriteLine(record);
            }
        }
    }

    private static bool TryGetFakeRealName(
            ISymbolDefinition head, List<LinkedListNode<SymbolRecord>> list, [MaybeNullWhen(false)] out string result)
        // TODO cache
    {
        result = default;

        var name = head.Name;

        if (!RegexFakeName().IsMatch(name))
        {
            return false;
        }

        var fake = list.Last().Next;

        if (fake is not { Value: ISymbolDefinition2 { Class: SymbolStorageClass.TPDEF, Type.Kind: SymbolTypeKind.STRUCT } def })
        {
            return false;
        }

        if (def.Size == head.Size && def.Tag == name)
        {
            result = def.Name;
        }

        return result != null;
    }

    private static LinkedListNode<TNode>? Search<TNode, TData>(
        LinkedListNode<TNode> node,
        Func<LinkedListNode<TNode>, LinkedListNode<TNode>?> next,
        Func<LinkedListNode<TNode>, TData, bool> predicate,
        TData userData)
    {
        var current = node;

        while (current != null)
        {
            if (predicate(current, userData))
            {
                return current;
            }

            current = next(current);
        }

        return null;
    }

    private static string GetStructString(List<LinkedListNode<SymbolRecord>> result)
    {
        var first = result.First();

        var head = first.Value as ISymbolDefinition ?? throw new InvalidOperationException();

        using var sw = new StringWriter();
        using var tw = new IndentedTextWriter(sw);

        if (!TryGetFakeRealName(head, result, out var name)) // TODO struct members, e.g. struct .7fake r0;
        {
            name = head.Name;
        }

        tw.WriteLine($"struct {name}");
        tw.WriteLine("{");

        tw.Indent = 1;

        for (var node = first.Next; node != null && node != result.Last(); node = node.Next)
        {
            var def1 = node.Value as ISymbolDefinition ?? throw new InvalidOperationException();

            var def2 = def1 as ISymbolDefinition2; // array and/or tagged

            var kindString = GetKindString(def1.Type.Kind, GetKindStringRemap);

            tw.Write(kindString);

            if (def2 != null && !string.IsNullOrWhiteSpace(def2.Tag))
            {
                var fake = Search(first, s => s.Previous, IsRealStructMemberName, def2)!;

                if (fake != null)
                {
                    var def3 = (ISymbolDefinition2)fake.Value;

                    tw.Write($" {def3.Name}");
                }
                else
                {
                    tw.Write($" {def2.Tag}");
                }
            }

            var mod = def1.Type.Modifiers.ToArray();

            var fcn = mod.Any(s => s == SymbolTypeModifier.FCN);

            if (fcn)
            {
                tw.Write(" (");
            }

            foreach (var modifier in mod)
            {
                if (modifier == SymbolTypeModifier.PTR)
                {
                    tw.Write("*");
                }
            }

            if (!fcn)
            {
                tw.Write(" ");
            }

            tw.Write($"{def1.Name}");

            if (fcn)
            {
                tw.Write(")(void)");
            }

            if (def2 != null)
            {
                var length = def2.Dimensions.Length;

                if (length != 0)
                {
                    Assert.AreEqual(length, mod.Count(s => s == SymbolTypeModifier.ARY));

                    foreach (var dimension in def2.Dimensions)
                    {
                        tw.Write($"[{dimension}]");
                    }
                }
            }

            tw.WriteLine(";");
        }

        tw.Indent = 0;
        tw.WriteLine("};");

        return sw.ToString();
    }

    private static bool IsRealStructMemberName(LinkedListNode<SymbolRecord> node, ISymbolDefinition2 data)
    {
        return node.Value is ISymbolDefinition2 { Class: SymbolStorageClass.TPDEF } d && d.Tag == data.Tag;
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

    private static string GetKindString(SymbolTypeKind kind, Func<SymbolTypeKind, string?>? func = null)
    {
        var text = func?.Invoke(kind);

        if (text != null)
        {
            return text;
        }

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
            SymbolTypeKind.STRUCT => "struct",
            SymbolTypeKind.UNION  => "union",
            SymbolTypeKind.ENUM   => null,
            SymbolTypeKind.MOE    => null,
            SymbolTypeKind.UCHAR  => "unsigned char",
            SymbolTypeKind.USHORT => "unsigned short",
            SymbolTypeKind.UINT   => "unsigned int",
            SymbolTypeKind.ULONG  => "unsigned long",
            _                     => null
        } ?? throw new NotSupportedException(kind.ToString());
    }

    private static string? GetKindStringRemap(SymbolTypeKind kind)
    {
        // TODO typedefs can be searched but what when multiple match? e.g. UCHAR is BBOOL or UBYTE

        // for now, this trivial mechanism allows one to override language keywords

        return kind switch
        {
            SymbolTypeKind.UCHAR  => "u_char",
            SymbolTypeKind.USHORT => "u_short",
            SymbolTypeKind.UINT   => "u_int",
            SymbolTypeKind.ULONG  => "u_long",
            _                     => null
        };
    }

    private static LinkedListNode<T>? Remove<T>(LinkedList<T> list, Func<T, bool> head, Func<T, bool> tail)
    {
        var first = list.First;

        if (first == null)
        {
            return null;
        }

        if (!TryFindNode(first, out var headNode, head))
        {
            return null;
        }

        if (!TryFindNode(headNode, out var tailNode, tail))
        {
            return null;
        }

        var node = headNode;

        while (node != null && node != tailNode)
        {
            var next = node.Next;

            list.Remove(node);

            node = next;
        }

        node = tailNode.Next;

        list.Remove(tailNode);

        return node;
    }

    private static void RemoveWhere<T>(LinkedList<T> list, Func<T, bool> predicate)
    {
        var current = list.First;

        while (current != null)
        {
            var next = current.Next;

            if (predicate(current.Value))
            {
                list.Remove(current);
            }

            current = next;
        }
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