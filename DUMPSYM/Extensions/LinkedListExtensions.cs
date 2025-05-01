using System.Diagnostics.CodeAnalysis;

namespace DUMPSYM.Extensions;

public static class LinkedListExtensions
{
    public static void CopyFrom<T>(this LinkedList<T> list, LinkedListNode<T> head, LinkedListNode<T> tail)
        // TODO this assumes next is the next one
    {
        if (head.List != tail.List)
        {
            throw new InvalidOperationException();
        }

        for (var node = head; node != null && node != tail.Next; node = node.Next)
        {
            list.AddLast(node.Value);
        }
    }

    public static LinkedListNode<T>? Remove<T>(this LinkedList<T> list, Func<T, bool> head, Func<T, bool> tail)
    {
        var first = list.First;

        if (first == null)
        {
            return null;
        }

        if (!first.TryFindNode(out var headNode, head))
        {
            return null;
        }

        if (!headNode.TryFindNode(out var tailNode, tail))
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

    public static void RemoveWhere<T>(this LinkedList<T> list, Func<T, bool> predicate)
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

    public static List<LinkedList<T>> Split<T>(this LinkedList<T> list, Func<T, bool> predicate)
    {
        var lists = new List<LinkedList<T>>();

        var node = list.First;

        while (node != null)
        {
            if (predicate(node.Value))
            {
                lists.Add(new LinkedList<T>());
            }

            lists[^1].AddLast(node.Value);

            node = node.Next;
        }

        return lists;
    }

    public static bool TryFindNode<T>(
        this LinkedListNode<T> node, [MaybeNullWhen(false)] out LinkedListNode<T> result, Func<T, bool> predicate)
    {
        return node.TryFindNode(s => s.Next, out result, predicate);
    }

    public static bool TryFindNode<T>(
        this LinkedListNode<T> node, Func<LinkedListNode<T>, LinkedListNode<T>?> next, [MaybeNullWhen(false)] out LinkedListNode<T> result, Func<T, bool> predicate)
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

            current = next(current);
        }

        return false;
    }

    public static bool TryFind<TNode, TItem>(
        this LinkedListNode<TNode> source,
        Func<LinkedListNode<TNode>, LinkedListNode<TNode>?> next,
        Func<TNode, TItem> selector,
        Func<TItem, bool> predicate,
        [MaybeNullWhen(false)] out TItem item,
        [MaybeNullWhen(false)] out LinkedListNode<TNode> node)
    {
        item = default;
        node = default;

        var current = source;

        while (current != null)
        {
            var data = selector(current.Value);

            if (predicate(data))
            {
                item = data;
                node = current;

                return true;
            }

            current = next(current);
        }

        return false;
    }
}