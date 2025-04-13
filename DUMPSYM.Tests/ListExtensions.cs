namespace DUMPSYM.Tests;

public static class ListExtensions
{
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