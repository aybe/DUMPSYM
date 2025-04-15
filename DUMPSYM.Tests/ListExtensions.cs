namespace DUMPSYM.Tests;

public static class ListExtensions
{
    public static void Remove<T>(this List<T> list, List<T> items)
    {
        items.ForEach(s => list.Remove(s));
    }

    public static void Remove<T>(this List<T> list, List<List<T>> items)
    {
        items.ForEach(list.Remove);
    }

    public static List<List<T>> Split<T>(this IEnumerable<T> list, Func<T, bool> predicate)
    {
        var lists = new List<List<T>>();

        foreach (var item in list)
        {
            if (predicate(item))
            {
                lists.Add([]);
            }

            lists[^1].Add(item);
        }

        return lists;
    }
}