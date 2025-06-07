namespace DUMPSYM.Extensions;

public static class CollectionExtensions
{
    public static IEnumerable<IEnumerable<T>> KCombinationWithRepetition<T>(this IEnumerable<T> source, int length)
    {
        ArgumentNullException.ThrowIfNull(source);

        ArgumentOutOfRangeException.ThrowIfNegative(length);

        var indices = new int[length];

        var array = source as T[] ?? source.ToArray();

        var pow = (int)Math.Pow(array.Length, length);

        for (var i = 0; i < pow; i++)
        {
            yield return indices.Select(s => array[s]);

            var indexer = length - 1;

            while (indexer >= 0)
            {
                if (indices[indexer] < array.Length - 1)
                {
                    indices[indexer]++;
                    break;
                }

                indices[indexer] = 0;
                indexer--;
            }

            if (indexer < 0)
            {
                yield break;
            }
        }
    }

    public static T[][] KCombinationWithRepetition<T>(this T[] modifiers, int length)
    {
        return modifiers.AsEnumerable().KCombinationWithRepetition(length).Select(s => s.ToArray()).ToArray(); // nice shit
    }

    public static bool TryGetRange<T>(this T[] array, int start, Predicate<T> header, Predicate<T> footer, out Range range)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(start);

        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(start, array.Length);

        range = default;

        if (!header(array[start]))
        {
            return false;
        }

        var index = Array.FindIndex(array, start, footer);

        if (index == -1)
        {
            return false;
        }

        range = start..(index + 1);

        return true;
    }

    public static bool TryGetTopologicalSort<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> selector, out List<T> result) where T : notnull
    {
        result = [];

        var array = source as T[] ?? source.ToArray();

        if (array.Length == 0)
        {
            return true;
        }

        var graph = array.ToDictionary(s => s, _ => new List<T>());

        var edges = array.ToDictionary(s => s, _ => 0);

        foreach (var element in array)
        {
            foreach (var dependency in selector(element))
            {
                graph[dependency].Add(element);

                edges[element]++;
            }
        }

        var queue = new Queue<T>(edges.Where(s => s.Value == 0).Select(s => s.Key));

        while (queue.Count > 0)
        {
            var dequeue = queue.Dequeue();

            result.Add(dequeue);

            foreach (var element in graph[dequeue])
            {
                if (--edges[element] == 0)
                {
                    queue.Enqueue(element);
                }
            }
        }

        var b = result.Count == edges.Count;

        result = b ? result : edges.Keys.Except(result).ToList();

        return b;
    }
}