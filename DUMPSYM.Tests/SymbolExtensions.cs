namespace DUMPSYM.Tests;

public static class SymbolExtensions // TODO move
{
    public static List<LinkedList<SymbolRecord>> GetTypes(LinkedList<SymbolRecord> records)
    {
        var types = new List<LinkedList<SymbolRecord>>();

        var current = records.First;

        while (current != null)
        {
            if (!TryFindNode(IsHeader, current, out current, out var header))
            {
                continue;
            }

            var headerNode = current!;

            if (!TryFindNode(IsFooter, current, out current, out var footer))
            {
                continue;
            }

            var footerNode = current!;

            Assert.AreEqual(header!.Name, footer!.Tag);

            var type = new LinkedList<SymbolRecord>();

            type.CopyFrom(headerNode, footerNode);

            types.Add(type);
        }

        return types;

        static ISymbolDefinition? IsHeader(SymbolRecord record)
        {
            return record is ISymbolDefinition { Class: SymbolStorageClass.STRTAG, Type.Kind: SymbolTypeKind.STRUCT } def ? def : null;
        }

        static ISymbolDefinition2? IsFooter(SymbolRecord record)
        {
            return record is ISymbolDefinition2 { Class: SymbolStorageClass.EOS, Type.Kind: SymbolTypeKind.NULL, Name: ".eos" } def ? def : null;
        }
    }

    private static bool TryFindNode<TNode, TResult>(
        Selector<TNode, TResult> selector, LinkedListNode<TNode>? from, out LinkedListNode<TNode>? next, out TResult? result)
    {
        next = default;

        result = default;

        for (var node = from; node != null; node = node.Next)
        {
            result = selector(node.Value);

            if (result == null)
            {
                continue;
            }

            next = node;

            return true;
        }

        return false;
    }

    private delegate T? Selector<in TNode, out T>(TNode node);
}