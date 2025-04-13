using System.Diagnostics.CodeAnalysis;

namespace DUMPSYM.Tests;

public static class LinkedListExtensions
{
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
}