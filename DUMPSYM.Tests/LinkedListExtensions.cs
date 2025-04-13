namespace DUMPSYM.Tests;

public static class LinkedListExtensions // TODO move more methods here
{
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
}